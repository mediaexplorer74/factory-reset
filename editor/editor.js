// COPYPAPSTA from Chunk.cs
var solids = {
    SolidPlatform:  0xFF000000, // A solid wall or platform
    HidingSpot:     0xFF404040, // A hiding spot for the player
    BackgroundWall: 0xFF808080, // A wall for enemies, but not the player
    FallThrough:    0xFFC0C0C0, // A jump/fall-through platform
    PlayerStart:    0xFF00FF00, // The start position for the player
    StaticCamera:   0xFFFF0100, // An enemy spawn position
    PivotCamera:    0xFFFF0200, // An enemy spawn position
    GroundDrone:    0xFFFF0300, // An enemy spawn position
    AerialDrone:    0xFFFF0400, // An enemy spawn position
    Spike:          0xFF0000FF, // A death spike
    Pickup:         0xFF00EEFF, // An information pickup item
    Goal:           0xFFFFEE00, // A goal tile leading to end-of-level
};

const tileSize = 16;
const defaultLayers = 4;
const tilesPerRow = 32;
const layerNames = ["Solids", "Background", "Ground", "Foreground", "Special"];
const minSize = [40,26];
var tilemap = document.querySelector("#tilemap");
var tilelist = document.querySelector("#tilelist");
var listctx, mapctx;
// Virtual
var tempcanvas = document.createElement("canvas");
var solidset = null;
var level = null;

/// Helper functions
var pad = function(string, length, char){
    char = char || " ";
    string = string+"";
    while(string.length < length){
        string = char + string;
    }
    return string;
};

var getPixel = function(data, x, y){
    var index = (x+y*data.width)*4;
    var r = data.data[index+0];
    var g = data.data[index+1];
    var b = data.data[index+2];
    var a = data.data[index+3];
    return ((r << 24) + (g << 16) + (b << 8) + a) >>> 0;
};

var formatRGB = function(pixel){
    var string = pixel.toString(16);
    return "#"+("00000000" + string).slice(-8);
};

var getImagePixels = function(image, dim){
    dim = dim || [image.width, image.height];
    tempcanvas.width = dim[0];
    tempcanvas.height = dim[1];
    var ctx = tempcanvas.getContext("2d");
    ctx.clearRect(0, 0, tempcanvas.width, tempcanvas.height);
    if(image) ctx.drawImage(image, 0, 0);
    return ctx.getImageData(0, 0, tempcanvas.width, tempcanvas.height);
};

var getImage = function(imagedata){
    tempcanvas.width = imagedata.width;
    tempcanvas.height = imagedata.height;
    var ctx = tempcanvas.getContext("2d");
    ctx.putImageData(imagedata, 0, 0);
    var image = new Image();
    
    return new Promise(function(accept){
        image.onload = function(){accept(image);};
        image.src = tempcanvas.toDataURL();
    });
};

var fillCheckerboard = function(canvas, ctx){
    var s = tileSize/2;
    ctx = ctx || canvas.getContext("2d");
    ctx.fillStyle = "#808080";
    ctx.fillRect(0, 0, canvas.width, canvas.height);
    ctx.fillStyle = "#C0C0C0";
    for (var y=0; y<canvas.height; y+=s) {
        for (var x=0; x<canvas.width; x+=s) {
            if(((y+x)/s)%2 == 0)
                ctx.rect(x, y, s, s);
        }
    }
    ctx.fill();
    return ctx;
};

var isTileEmpty = function(data, x, y){
    for(var iy=y*tileSize; iy<(y+1)*tileSize; iy++){
        for(var ix=x*tileSize; ix<(x+1)*tileSize; ix++){
            // Read alpha component
            if(data.data[(ix+iy*data.width)*4+3] != 0)
                return false;
        }
    }
    return true;
};

var loadFile = function(input, type){
    var f = input.files[0];
    var fr = new FileReader();

    return new Promise(function(accept){
        fr.onload = function(){
            accept(fr.result);
        };
        switch(type){
        case "data": fr.readAsDataURL(f); break;
        case "text": fr.readAsText(f); break;
        case "binary": fr.readAsBinaryString(f); break;
        case "array": fr.readAsArrayBuffer(f); break;
        }
    });
};

var loadImage = function(data){
    var image = new Image();
    return new Promise(function(accept){
        image.onload = function(){
            accept(image);
        };
        image.src = data;
    });
};

var openFile = function(type){
    var input = document.createElement("input");
    input.type = "file";
    input.accept = type;

    return new Promise(function(accept){
        input.addEventListener("change", function(){
            accept(input);
        });
        input.click();
    });
};

var saveFile = function(data, filename){
    var link = document.createElement("a");
    data = data.replace(/data:.*?;/, "data:application/octet-stream;");
    link.setAttribute("download", filename || "file.dat");
    link.setAttribute("href", data);
    link.click();
};

/// Base classes
class Tileset{
    constructor(init){
        init = init || {};
        this.image = null;
        this.pixels = null;
        this.name = init.name || "?";
        this.rgMap = init.rgMap || {};
        this.tileMap = init.tileMap || [];
        this.currentTile = [0, 0];
        
        if(init.image || init.pixels)
            this.preprocess(init.image, init.pixels);
    }

    preprocess(image, pixels){
        if(!pixels)
            pixels = getImagePixels(image);
        if(!image){
            return getImage(pixels)
                .then((image)=>this.preprocess(image, pixels));
        }
        // First compute maps
        var tileW = pixels.width / tileSize;
        var tileH = pixels.height / tileSize;
        this.tileMap[0] = new Array(tilesPerRow);
        var tx=0, ty=0;
        for(var iy=0; iy<tileH; iy++){
            for(var ix=0; ix<tileW; ix++){
                if(!isTileEmpty(pixels, ix, iy)){
                    this.tileMap[ty][tx] = [ix, iy];
                    this.rgMap[(ix<<8) + iy] = [tx, ty];
                    tx++;
                    if(tilesPerRow <= tx){
                        tx = 0;
                        ty++;
                        this.tileMap[ty] = new Array(tilesPerRow);
                    }
                }
            }
        }
        // Then compute static, compressed image
        tempcanvas.width = this.tileMap[0].length*tileSize;
        tempcanvas.height = this.tileMap.length*tileSize;
        var ctx = fillCheckerboard(tempcanvas);
        for(var iy=0; iy<this.tileMap.length; iy++){
            for(var ix=0; ix<this.tileMap[iy].length; ix++){
                var rg = this.tileMap[iy][ix];
                if(rg){
                    ctx.drawImage(image, rg[0]*tileSize, rg[1]*tileSize, tileSize, tileSize,
                                  ix*tileSize, iy*tileSize, tileSize, tileSize);
                }
            }
        }
        this.pixels = ctx.getImageData(0, 0, tempcanvas.width, tempcanvas.height);
        this.image = new Image();
        this.image.src = tempcanvas.toDataURL();
        return this;
    }

    get selected(){ return this.currentTile; }
    set selected(value){
        var x = value[0];
        var y = value[1];
        y = (y<0)? 0 : (this.tileMap.length <= y) ? this.tileMap.length-1 : y;
        x = (x<0)? 0 : (this.tileMap[y].length <= x) ? this.tileMap[y].length-1 : x;
        this.currentTile = [x, y];

        var tile = this.tileMap[y][x];
        var format = formatRGB(getPixel(this.pixels, x*tileSize, y*tileSize));
        document.querySelector("#selected").innerText = pad(tile[0], 3)+","+pad(tile[1], 3);
        document.querySelector("#color").innerText = format;
        console.log(this, "Selected tile", this.currentTile, tile, format);
        return this.currentTile;
    }

    show(){
        console.log("Showing", this);
        listctx.drawImage(this.image, 0, 0);
        return this;
    }

    use(){
        console.log(this, "Using.");
        level.chunk.tileset = this;
        level.chunk.show();
        return this;
    }
}

class Chunk{
    constructor(init){
        init = init || {};
        this.name = init.name || "chunk";
        this.position = init.position || [0, 0];
        this.currentLayer = 0;
        this.tileset = init.tileset || level.defaultTileset;
        this.storyItems = init.storyItems || [];
        this.pixels = init.pixels;
        
        if(!this.pixels){
            var layers = init.layers || defaultLayers;
            this.pixels = new Array(layers);
            for(var i=0; i<layers; i++){
                this.pixels[i] = new ImageData(init.width || minSize[0], init.height || minSize[1]);
                this.pixels[i].show = true;
            }
        }
    }

    get width(){ return this.pixels[0].width; }
    get height(){ return this.pixels[0].height; }
    get layers(){ return this.pixels.length; }
    get layer(){ return this.pixels[this.currentLayer]; }
    
    getTileset(layer){
        return (layer && 0 < layer)? this.tileset: solidset;
    }

    drawPos(x, y){
        var pixelIndex = ((this.width*y)+x)*4;
        var empty = true;
        for(var l=0; l<this.pixels.length; l++){
            var pixels = this.pixels[l];
            var tileset = this.getTileset(l);
            if(pixels.show){
                var r = pixels.data[pixelIndex+0];
                var g = pixels.data[pixelIndex+1];
                var a = pixels.data[pixelIndex+3];
                if(0 < a){
                    empty = false;
                    var s = tileset.rgMap[(r<<8) + g];
                    mapctx.drawImage(tileset.image,
                                     s[0]*tileSize, s[1]*tileSize, tileSize, tileSize,
                                     x*tileSize, y*tileSize, tileSize, tileSize);
                }
            }
        }
        if(empty){
            mapctx.fillRect(x*tileSize, y*tileSize, tileSize, tileSize);
            mapctx.strokeStyle = "#DDDDDD";
            mapctx.beginPath();
            mapctx.moveTo(x*tileSize+0.5, (y+1)*tileSize-0.5);
            mapctx.lineTo(x*tileSize+0.5, y*tileSize+0.5);
            mapctx.lineTo((x+1)*tileSize-0.5, y*tileSize+0.5);
            mapctx.stroke();
        }
        return this;
    }

    clear(){
        console.log("Clearing", this);
        tilemap.width = this.width*tileSize;
        tilemap.height = this.height*tileSize;
        mapctx.fillStyle = "#FFFFFF";
        mapctx.strokeStyle = "#DDDDDD";
        mapctx.fillRect(0, 0, tilemap.width, tilemap.height);
        // FIXME: clear pixels
        for(var y=0; y<tilemap.height; y+=tileSize){
            mapctx.beginPath();
            mapctx.moveTo(0, y+0.5);
            mapctx.lineTo(tilemap.width, y+0.5);
            mapctx.stroke();
        }
        for(var x=0; x<tilemap.width; x+=tileSize){
            mapctx.beginPath();
            mapctx.moveTo(x+0.5, 0);
            mapctx.lineTo(x+0.5, tilemap.height);
            mapctx.stroke();
        }
        return this;
    }

    show(){
        console.log("Showing", this);
        tilemap.width = this.width*tileSize;
        tilemap.height = this.height*tileSize;
        mapctx.fillStyle = "#FFFFFF";
        mapctx.fillRect(0, 0, tilemap.width, tilemap.height);
        for(var y=0; y<this.height; y+=1){
            for(var x=0; x<this.width; x+=1){
                this.drawPos(x, y);
            }
        }
        return this;
    }

    layerImage(layer){
        layer = layer || this.currentLayer;
        var pixels = this.pixels[layer];
        tempcanvas.width = pixels.width;
        tempcanvas.height = pixels.height;
        tempcanvas.getContext("2d").putImageData(pixels, 0, 0);
        return tempcanvas.toDataURL("image/png");
    }

    saveLayer(layer){
        layer = layer || this.currentLayer;
        var data = this.layerImage(layer);
        return saveFile(data, this.name+"-"+layer+".png");
    }

    loadLayer(layer){
        layer = layer || this.currentLayer;
        return openFile(".png,image/png")
            .then(input => loadFile(input, "data"))
            .then(data => loadImage(data))
            .then(image => {
                var show = this.pixels.show;
                this.pixels[layer] = getImagePixels(image);
                this.pixels[layer].show = show;
                show();
            });
    }

    edit(x, y, action, layer){
        layer = layer || this.currentLayer;
        var pixels = this.pixels[layer];
        var pixelIndex = ((pixels.width*y)+x)*4;
        if(action === "place"){
            var tileset = this.getTileset(layer);
            var tile = tileset.tileMap[tileset.selected[1]][tileset.selected[0]];
            pixels.data[pixelIndex+0] = tile[0];
            pixels.data[pixelIndex+1] = tile[1];
            pixels.data[pixelIndex+3] = 255;
        }else if(action === "erase"){
            pixels.data[pixelIndex+0] = 0;
            pixels.data[pixelIndex+1] = 0;
            pixels.data[pixelIndex+3] = 0;
        }
        this.drawPos(x,y);
        console.log(this, "Edited (",x,"x",y,")");
        return this;
    }

    serialize(){
        return {
            name: this.name,
            position: this.position,
            layers: this.layers,
            tileset: this.tileset.name,
            storyItems: this.storyItems
        };
    }

    use(){
        console.log("Using", this);
        level.chunk = this;
        this.getTileset().show();
        return this.show();
    }
}

class Level{
    constructor(init){
        init = init || {};
        this.name = init.name || "level";
        this.description = init.description || "";
        this.startChase = init.startChase || false;
        this.startChunk = init.startChunk || 0;
        this.defaultTileset = init.defaultTileset || solidset;
        this.chunks = init.chunks || [ new Chunk({tileset: this.defaultTileset}) ];
        this.currentChunk = 0;
    }

    get chunk(){ return this.chunks[this.currentChunk]; }
    set chunk(chunk) {
        var index = (chunk instanceof Chunk)? this.chunks.indexOf(chunk) : chunk;
        this.currentChunk = index;
        return this.chunk.show();
    }

    serialize(){
        var chunks = [];
        for(var i=0; i<this.chunks.length; i++){
            chunks.push(this.chunks[i].serialize());
        }
        return {
            name: this.name,
            description: this.description,
            startChase: this.startChase,
            startChunk: this.startChunk,
            chunks: chunks
        };
    }

    use(){
        console.log("Using", this);
        level = this;
        this.chunk.use();
        return this;
    }
}

/// UI
var createSolidTileset = function(){
    var tileToRG = [[]];
    var RGToTile = {};
    var pixels = new ImageData(tileSize*tilesPerRow, tileSize);
    var x=0, y=0;
    for(var key in solids){
        var color = solids[key];
        var r = (color & 0x000000FF) >>>  0;
        var g = (color & 0x0000FF00) >>>  8;
        var b = (color & 0x00FF0000) >>> 16;
        var a = (color & 0xFF000000) >>> 24;
        tileToRG[0][x] = [r, g];
        RGToTile[(r<<8)+g] = [x, 0];
        for(var i=y*tileSize; i<(y+1)*tileSize; i++){
            for(var j=x*tileSize; j<(x+1)*tileSize; j++){
                var index = (j+i*pixels.width)*4;
                pixels.data[index+0] = r;
                pixels.data[index+1] = g;
                pixels.data[index+2] = b;
                pixels.data[index+3] = a;
            }
        }
        x++;
    }
    return getImage(pixels)
        .then(function(image){
            return new Tileset({
                name: "solids",
                pixels: pixels,
                image: image,
                tileMap: tileToRG,
                rgMap: RGToTile
            });
        });
};

var selectTileEvent = function(ev){
    var tileset = level.chunk.getTileset();
    if(ev instanceof WheelEvent){
        var delta = -Math.sign(ev.deltaY);
        var i = tileset.selected[0]+tileset.selected[1]*tilesPerRow;
        i += delta;
        tileset.selected = [i % tilesPerRow, Math.floor(i / tilesPerRow)];
    }else if(ev instanceof MouseEvent){
        var x = Math.floor(ev.offsetX/tilelist.clientWidth*tilelist.width/tileSize);
        var y = Math.floor(ev.offsetY/tilelist.clientHeight*tilelist.height/tileSize);
        tileset.selected = [x, y];
    }
};

var button = 0;
var editMapEvent = function(ev){
    if(ev.buttons){
        var x = Math.floor(ev.offsetX/tileSize);
        var y = Math.floor(ev.offsetY/tileSize);
        var action = (button == 2)? "erase" : "place";
        level.chunk.edit(x, y, action);
    }
};

var newChunk = function(){
    var prompt = document.querySelector("#chunk-prompt");
    prompt.style.display = "block";
    prompt.querySelector("input[type=submit]").onclick = function(ev){
        if(prompt.checkValidity()){
            prompt.style.display = "none";
            var name = prompt.querySelector("#chunk-name").value;
            var w = parseInt(prompt.querySelector("#chunk-width").value);
            var h = parseInt(prompt.querySelector("#chunk-height").value);
            var complete = function(tileset){
                level.chunks.push(
                    new Chunk({
                        name: name,
                        width: w,
                        height: h,
                        tileset: tileset,
                    }));
            };
            if(prompt.querySelector("#chunk-tileset").value)
                loadFile(prompt.querySelector("#chunk-tileset"))
                .then(data => loadImage(data))
                .then(image => complete(new Tileset({image: image})));
            else
                complete(null);
            ev.preventDefault();
        }
        return false;
    };
};

var newLevel = function(){
    var prompt = document.querySelector("#level-prompt");
    prompt.style.display = "block";
    prompt.querySelector("input[type=submit]").onclick = function(ev){
        if(prompt.checkValidity()){
            prompt.style.display = "none";
            var name = prompt.querySelector("#level-name").value;
            var description = prompt.querySelector("#level-description").value;
            var startChase = prompt.querySelector("#level-startchase").value;
            loadFile(prompt.querySelector("#level-tileset"))
                .then(data => loadImage(data))
                .then(image => new Level({
                    name: name,
                    description: description,
                    startChase: startChase,
                    defaultTileset: new Tileset({image: image}),
                }).use());
            ev.preventDefault();
        }
        return false;
    };
};

var openLevel = function(){
    return openFile(".zip,application/zip")
        .then(input => loadFile(input, "array"))
        .then(data => {
            var zip = new JSZip();
            var chunks = {};
            return zip.loadAsync(data)
                .then(zip => zip.file("level.json").async("string"))
                .then(async data => {
                    var json = JSON.parse(data);
                    for(var i=0; i<json.chunks.length; i++){
                        var chunk = json.chunks[i];
                        chunk.pixels = [];
                        chunk.tileset = solidset;
                        for(var l=0; l<chunk.layers; l++){
                            var base64 = await zip.file("chunks/"+chunk.name+"-"+l+".png").async("base64");
                            var image = await loadImage("data:image/png;base64,"+base64);
                            chunk.pixels[l] = getImagePixels(image);
                        }
                        json.chunks[i] = new Chunk(chunk);
                    }
                    return new Level(json).use();
                });});
};

var saveLevel = function(){
    var zip = new JSZip();
    var chunks = zip.folder("chunks");
    for(var chunk of level.chunks){
        for(var l=0; l<chunk.layers; ++l){
            var data = chunk.layerImage(l).replace(/.*?base64,/, "");
            chunks.file(chunk.name+"-"+l+".png", data, {"base64":true});
        }
    }
    zip.file("level.json", JSON.stringify(level.serialize()));
    zip.generateAsync({type:"base64"}).then(function(data){
        data = "data:application/zip;base64,"+data;
        return saveFile(data, level.name+".zip");
    });
};

var initEvents = function(){
    document.querySelector("#new-level").addEventListener("click", newLevel);
    document.querySelector("#open-level").addEventListener("click", openLevel);
    document.querySelector("#save-level").addEventListener("click", saveLevel);
    document.querySelector("#new-chunk").addEventListener("click", newChunk);
    window.addEventListener("wheel", selectTileEvent);
    tilelist.addEventListener("click", selectTileEvent);
    tilemap.addEventListener("mousedown", function(ev){button = ev.button; editMapEvent(ev);});
    tilemap.addEventListener("mousemove", editMapEvent);
    tilemap.addEventListener("contextmenu", function(ev){ev.preventDefault();});
};

var initCanvas = function(){
    mapctx = tilemap.getContext("2d");
    mapctx.mozImageSmoothingEnabled = false;
    mapctx.webkitImageSmoothingEnabled = false;
    mapctx.msImageSmoothingEnabled = false;
    mapctx.imageSmoothingEnabled = false;
    
    listctx = tilelist.getContext("2d");
    listctx.mozImageSmoothingEnabled = false;
    listctx.webkitImageSmoothingEnabled = false;
    listctx.msImageSmoothingEnabled = false;
    listctx.imageSmoothingEnabled = false;
};

var init = function(){
    console.log("Init.");
    initEvents();
    initCanvas();
    
    createSolidTileset()
        .then(function(tileset){
            solidset = tileset;
            new Level().use();
        });
};

init();
