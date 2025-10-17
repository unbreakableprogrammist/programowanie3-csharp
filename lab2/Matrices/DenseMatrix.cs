namespace Matrices.Matrix;

public class DenseMatrix : Matrix
{
    private float[,] _data;

    public DenseMatrix(int rows, int columns) : base(rows, columns)
    {
        _data = new float[rows, columns];
    }
    public override float this[int row, int column]
    {
        get
        {
            if (row < 0 || row >= Rows || column < 0 || column >= Columns) 
                throw new ArgumentException("Index out of range.");
            return _data[row,column];
        }
        set
        {
            if  (row < 0 || row >= Rows || column < 0 || column >= Columns)
                throw new ArgumentException("Index out of range.");
                _data[row, column] = value;
        }
    }
    public override Matrix Transpose()
    {
        DenseMatrix result = new DenseMatrix(Rows, Columns);
        for (int row = 0; row < Rows; row++)
        {
            for (int col = 0; col < Columns; col++)
            {
                result[row,col] = this[col,row];
            }
        }
        return result;
    }

    public override void Identity()
    {
        if (Rows != Columns)
        {
            throw new ArgumentException("Matrix size does not match.");
        }

        for (int i = 0; i < Rows; i++)
        {
            for (int j = 0; j < Columns; j++)
            {
                if (i == j)
                {
                    _data[i, j] = 1.0f;
                }
                else
                {
                    _data[i, j] = 0.0f;
                }
            }
        }
    }

    public override float Norm()
    {
        float result =  0.0f;
        for (int i = 0; i < Rows; i++)
        {
            for (int j = 0; j < Columns; j++)
            {
                result += this[i, j] * this[i, j];
            }
        }
        return MathF.Sqrt(result);
        
    }
    protected override Matrix GetInstance(int rows, int cols)
    {
        return new DenseMatrix(rows, cols);
    }

    
}