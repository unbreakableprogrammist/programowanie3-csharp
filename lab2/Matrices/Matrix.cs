using System.Text;

namespace Matrices.Matrix;

public abstract class Matrix
{
    public int Rows { get; }
    public int Columns { get;}

    protected Matrix(int rows, int columns)
    {
        if (rows <= 0 || columns <= 0)
            throw new ArgumentException("Row and Column count must be greater than zero");
        Rows = rows;
        Columns = columns;
    }
    public abstract float this[int row, int column] { get; set;}
    public abstract Matrix Transpose();
    public abstract void Identity();
    public abstract float Norm();
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