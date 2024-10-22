import java.io.File;

public class SpriteImporter{
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
                    
                    Pixel[][] newImg = new Pixel[img.length*2 - 16][img[0].length];
                    for(int row = 0; row < img.length; row++)
                        for(int col = 0; col < img[0].length; col++)
                            newImg[row][col] = img[row][col];

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
}