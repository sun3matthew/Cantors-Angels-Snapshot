public class Pixel
{
  int r;
  int g;
  int b;
  int a;
  public Pixel()
  {
    r = 0;
    g = 0;
    b = 0;
    a = 255;
  }
  public Pixel(int rIn, int gIn, int bIn, int aIn)
  {
    this();
    setColor(rIn, gIn, bIn, aIn);
  }
  public Pixel(int rIn, int gIn, int bIn)
  {
    this(rIn, gIn, bIn, 255);
  }
  public Pixel(Pixel in)
  {
    this(in.r, in.g, in.b, in.a);
  }
  public void setColor(int rIn, int gIn, int bIn, int aIn)
  {
    if(rIn >= 255)
      r = 255;
    else if(rIn < 0)
      r = 0;
    else
      r = rIn;
    if(gIn >= 255)
      g = 255;
    else if(gIn < 0)
      g = 0;
    else
      g = gIn;
    if(bIn >= 255)
      b = 255;
    else if(bIn < 0)
      b = 0;
    else
      b = bIn;
    if(aIn >= 255)
      a = 255;
    else if(aIn < 0)
      a = 0;
    else
      a = aIn;
  }
  public void add(Pixel other){
    setColor(r+other.r, g+other.g, b+other.b, a+other.a);
  }
  public int comparePixel(Pixel compareThis)
  {
    return (Math.abs(compareThis.r-this.r)+Math.abs(compareThis.g-this.g)+Math.abs(compareThis.b-this.b))/3;
  }
}
