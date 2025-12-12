using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace FractalsGenerator.Generators.MandelbrotSet.Implementations;

public sealed class MultiThreadGenerator(int maxIterations)   // sealed bo nie mozna po nas dziedziczyc
    : MandelbrotSetGenerator(maxIterations){  // to ze przekazujemy maxIterations to taki skrotowy konstruktor
    private void Thread_work(Image<Rgba32> image,int start,int end,int height,int width)
    {
        for (var y = 0; y < height; y++)
        {
            for (var x = start; x < end; x++)
            {
                var a = (x - width / 2.0) * 4.0 / width;
                var b = (y - height / 2.0) * 4.0 / height;

                var iterations = Calculate(a, b);
                image[x, y] = GetColor(iterations);
            }
        }
    }
    protected override void PopulateImage(Image<Rgba32> image)
    {
        var width = image.Width;
        var height = image.Height;
        var ile_rdzeni = Environment.ProcessorCount; // liczymy ile mam rdzeni w kompie
        var width_per_thread = width / ile_rdzeni;
        List<Thread> watki = new List<Thread>();
        for (var i = 0; i < ile_rdzeni; i++)
        {
            int start = i*width_per_thread;
            int end = start + width_per_thread;
            if(i==ile_rdzeni-1) end = width;
            Thread watek = new Thread(()=>Thread_work(image,start,end,height,width));
            watki.Add(watek);
            watki[i].Start();
        }
        foreach (var thread in watki)
        {
            thread.Join();
        }
        
        
    }
}