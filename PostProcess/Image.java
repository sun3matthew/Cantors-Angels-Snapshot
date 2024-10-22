import javax.imageio.ImageIO;
import java.awt.image.BufferedImage;
import java.io.File;
import java.io.IOException;

/*
This class helps read an image in and maps it into a
2d array, and can also create a image given a 2d array of
Pixles.

By: Matthew Sun
Since: May 31 2020
*/
public class Image
{
  public static void exportImage(String name, String fileType, Pixel[][] pixelArray)
  {
    try{
      if(fileType.equals("JPG")){
        BufferedImage exportThis = new BufferedImage(pixelArray[0].length, pixelArray.length, BufferedImage.TYPE_INT_RGB);
        for (int row = 0; row < pixelArray.length; row++)
          for (int col = 0; col < pixelArray[0].length; col++)
            exportThis.setRGB(col, row, (((pixelArray[row][col].r & 0xFF) << 16)|((pixelArray[row][col].g & 0xFF) << 8)|((pixelArray[row][col].b & 0xFF) << 0)));
        File outputFile = new File(name+ "." + fileType);
        ImageIO.write(exportThis, fileType, outputFile);
      }else{
        BufferedImage exportThis = new BufferedImage(pixelArray[0].length, pixelArray.length, BufferedImage.TYPE_INT_ARGB);
        for (int row = 0; row < pixelArray.length; row++)
          for (int col = 0; col < pixelArray[0].length; col++)
            exportThis.setRGB(col, row, (((pixelArray[row][col].a & 0xFF) << 24)|((pixelArray[row][col].r & 0xFF) << 16)|((pixelArray[row][col].g & 0xFF) << 8)|((pixelArray[row][col].b & 0xFF) << 0)));
        File outputFile = new File(name+ "." + fileType);
        ImageIO.write(exportThis, fileType, outputFile);
      }
    }catch (IOException e)
    {
      System.out.println("There was a error exporting the Image:\n" + e);
    }
  }
  public static void exportImage(String name, Pixel[][] pixelArray, String type)
  {
    exportImage(name, type, pixelArray);
  }
  public static void exportImage(String name, Pixel[][] pixelArray)
  {
    exportImage(name, "png", pixelArray);
  }
  public static Pixel[][] readImage(String name){
    BufferedImage picture;
    Pixel[][] pixelArray;
    try{
      picture = ImageIO.read(new File(name));
    }catch (Exception e)
    {
      System.out.println("There was a error reading the picture\n" + e);
      return null;
    }
    pixelArray = new Pixel[picture.getHeight()][picture.getWidth()];
    for (int row = 0; row < pixelArray.length; row++)
    {
      for (int col = 0; col < pixelArray[0].length; col++)
      {
        int value = picture.getRGB(col, row);
        int alpha = (value >> 24) & 0xff;
        int red = (value >> 16) & 0xff;
        int green = (value >>  8) & 0xff;
        int blue = value & 0xff;
        pixelArray[row][col] = new Pixel(red, green, blue, alpha);
      }
    }
    return pixelArray;
  }
}
