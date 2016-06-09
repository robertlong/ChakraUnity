const width = 10;
const depth = 10;
const height = 10;

for (var x = 0; x < width; x++) {
    for (var y = 0; y < depth; y++) {
        for (var z = 0; z < height; z++) {
            if ((x % 2) + (y % 2) + (z % 2) === 0) {
                const red = 255 * (x / width);
                const green = 255 * (y / depth);
                const blue = 255 * (z / height);

                scene.createCube(x, y, z, red, green , blue, 1);
            }            
        }
    }
}
