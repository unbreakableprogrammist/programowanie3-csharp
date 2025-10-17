using System.Text;

namespace Matrices.Matrix;

public abstract class Matrix
{
    public int Rows { get; }
    public int Columns { get;}

    protected Matrix(int rows, int columns)
    {
        Rows = rows;
        Columns = columns;
    }
    public abstract float this[int row, int column] { get; set;}

    public override string ToString()
    {
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < Rows; i++)
        {
            for (int j = 0; j < Columns; j++)
            {
                sb.Append(this[i, j]);
            }
        }
    }
}