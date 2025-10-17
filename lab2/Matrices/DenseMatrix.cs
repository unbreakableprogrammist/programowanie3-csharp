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
}