import java.io.File;

public class HexTile{
    //Replaces all the files with a new version with the entity offset by itself, needs name + animation.
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

                    //left bound
                    int leftBound = 0;
                    for(int col = 0; col < img[0].length; col++)
                    {
                        boolean found = false;
                        for(int row = 0; row < img.length; row++)
                        {
                            if(img[row][col].a != 0)
                            {
                                leftBound = col;
                                found = true;
                                break;
                            }
                        }
                        if(found)
                            break;
                    }

                    //right bound
                    int rightBound = img[0].length - 1;
                    for(int col = img[0].length - 1; col >= 0; col--)
                    {
                        boolean found = false;
                        for(int row = 0; row < img.length; row++)
                        {
                            if(img[row][col].a != 0)
                            {
                                rightBound = col;
                                found = true;
                                break;
                            }
                        }
                        if(found)
                            break;
                    }

                    //top bound
                    int topBound = 0;
                    for(int row = 0; row < img.length; row++)
                    {
                        boolean found = false;
                        for(int col = 0; col < img[0].length; col++)
                        {
                            if(img[row][col].a != 0)
                            {
                                topBound = row;
                                found = true;
                                break;
                            }
                        }
                        if(found)
                            break;
                    }

                    //bottom bound
                    int bottomBound = img.length - 1;
                    for(int row = img.length - 1; row >= 0; row--)
                    {
                        boolean found = false;
                        for(int col = 0; col < img[0].length; col++)
                        {
                            if(img[row][col].a != 0)
                            {
                                bottomBound = row;
                                found = true;
                                break;
                            }
                        }
                        if(found)
                            break;
                    }

                    Pixel[][] newImg = new Pixel[bottomBound - topBound + 1][rightBound - leftBound + 1];
                    for(int row = topBound; row <= bottomBound; row++)
                        for(int col = leftBound; col <= rightBound; col++)
                            newImg[row - topBound][col - leftBound] = img[row][col];


                    // int HexTileHeight = 28; // extra pixels to extend downwards
                    // // at each column, add pixels to the bottom
                    // Pixel[][] newImg2 = new Pixel[newImg.length + HexTileHeight][newImg[0].length];
                    // for(int row = 0; row < newImg.length; row++)
                    //     for(int col = 0; col < newImg[0].length; col++)
                    //         newImg2[row][col] = newImg[row][col];

                    // // add 10 pixels of the same color of the bottom most non null pixel or non transparent pixel
                    // for(int col = 0; col < newImg2[0].length; col++)
                    // {
                    //     int bottomRow = newImg2.length - 1;
                    //     for(int row = newImg2.length - 1; row >= 0; row--)
                    //     {
                    //         if(newImg2[row][col] != null && newImg2[row][col].a != 0)
                    //         {
                    //             bottomRow = row;
                    //             break;
                    //         }
                    //     }
                    //     int max = bottomRow + HexTileHeight;
                    //     for(int row = bottomRow + 1; row <= max; row++){
                    //         // if its in the left half, then its should be slightly darker, if its in the right half, then it should be more darker
                    //         if(col < newImg2[0].length / 2){
                    //             newImg2[row][col] = darken(newImg2[bottomRow][col], 0.8);
                    //         }else{
                    //             newImg2[row][col] = darken(newImg2[bottomRow][col], 0.6);
                    //         }
                    //     }
                    // }
                    
                    // newImg = newImg2;

                    // // add same amount of pixels to the top to keep center
                    // Pixel[][] newImg3 = new Pixel[newImg.length + HexTileHeight][newImg[0].length];
                    // for(int row = 0; row < newImg.length; row++)
                    //     for(int col = 0; col < newImg[0].length; col++)
                    //         newImg3[row + HexTileHeight][col] = newImg[row][col];

                    // newImg = newImg3;

                    for(int row = 0; row < newImg.length; row++)
                        for(int col = 0; col < newImg[0].length; col++)
                            if(newImg[row][col] == null)
                                newImg[row][col] = new Pixel(0, 0, 0, 0);
                    listOfFiles[i].delete();
                    Image.exportImage(basePath + args[1] + "/" + listOfFiles[i].getName().substring(0, listOfFiles[i].getName().indexOf('.')), newImg);
                }else if(listOfFiles[i].getPath().indexOf(".meta") == -1){
                    listOfFiles[i].delete();
                }
            }
        }
        else
        {
            System.out.println("You need the name of the unit and animation");
        }
    }
    public static Pixel darken(Pixel p, double factor){
        return new Pixel((int)(p.r * factor), (int)(p.g * factor), (int)(p.b * factor), p.a);
    }
}