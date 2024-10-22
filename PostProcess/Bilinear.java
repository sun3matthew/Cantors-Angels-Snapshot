import java.io.File;

public class Bilinear{

    public static final int LEVELS_OF_ZOOM = 5;
    public static final float LARGEST_ZOOM = 0.1f;//10 percent of the original size
    //Replaces the file with a new version with , needs name + animation.
    public static void main(String[] args){
        if(args.length > 1)
        {
            String basePath = "/Users/mattsun/Cantors-Angels/Assets/Resources/Sprites/" + args[0] + "/";
            File[] listOfFiles = new File(basePath + args[1] + "/").listFiles();
            for(int i = 0; i < listOfFiles.length; i++)
            {
                if(listOfFiles[i].getPath().indexOf(".meta") == -1 && listOfFiles[i].getPath().indexOf(".DS_Store") == -1){
                    System.out.println(listOfFiles[i].getPath());
                    Pixel[][] img = Image.readImage(listOfFiles[i].getPath());
                    
                    // Pixel[][] newImg = new Pixel[img.length*2 - 16][img[0].length];
                    // for(int row = 0; row < img.length; row++)
                    //     for(int col = 0; col < img[0].length; col++)
                    //         newImg[row][col] = img[row][col];

                    // for(int row = 0; row < newImg.length; row++)
                    //     for(int col = 0; col < newImg[0].length; col++)
                    //         if(newImg[row][col] == null)
                    //             newImg[row][col] = new Pixel(0, 0, 0, 0);

                    Pixel[][] oldImg = img;
                    for(int zoom = 0; zoom < LEVELS_OF_ZOOM; zoom++)
                    {
                        Pixel[][] newImg = new Pixel[img.length][img[0].length];
                        // lower resolution by interpolating between pixels
                        for(int row = 0; row < newImg.length; row++){
                            for(int col = 0; col < newImg[0].length; col++)
                            {
                                if(img[row][col].a == 0){
                                    newImg[row][col] = new Pixel(0, 0, 0, 0);
                                    continue;
                                }
                                    
                                int kernelSize = zoom + 1;
                                int actualRow = (row / kernelSize) * kernelSize;
                                int actualCol = (col / kernelSize) * kernelSize;
                                int r = 0;
                                int g = 0;
                                int b = 0;
                                int numPixels = 0;
                                for(int kRow = actualRow; kRow < actualRow + kernelSize; kRow++){
                                    for(int kCol = actualCol; kCol < actualCol + kernelSize; kCol++){
                                        if(kRow >= img.length || kCol >= img[0].length)
                                            continue;
                                        if(img[kRow][kCol].a == 0)
                                            continue;
                                        r += img[kRow][kCol].r;
                                        g += img[kRow][kCol].g;
                                        b += img[kRow][kCol].b;
                                        numPixels++;
                                    }
                                }
                                r /= numPixels;
                                g /= numPixels;
                                b /= numPixels;
                                newImg[row][col] = new Pixel(r, g, b, 255);
                            }
                        }
                        Image.exportImage(basePath + args[1] + "/" + listOfFiles[i].getName().substring(0, listOfFiles[i].getName().indexOf('.')) + "_" + zoom, newImg);
                    }
                    //Image.exportImage(basePath + args[1] + "/" + listOfFiles[i].getName().substring(0, listOfFiles[i].getName().indexOf('.')), newImg);

                    listOfFiles[i].delete();
                }else if(listOfFiles[i].getPath().indexOf(".meta") != -1){
                    //listOfFiles[i].delete();
                }
            }
        }
        else
        {
            System.out.println("You need the name of the unit and animation");
        }
    }
}