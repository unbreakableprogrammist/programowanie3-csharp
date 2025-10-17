namespace Matrices.Matrix;

public class SparseMatrix : Matrix
{
    private Dictionary<(int row, int column), float> _data;

    public SparseMatrix(int rows, int columns) : base(rows, columns)
    {
        _data = new Dictionary<(int row, int column), float>();
    }

    public override float this[int row, int column]
    {
        get
        {
            if (row < 0 || row >= Rows || column < 0 || column >= Columns) 
                throw new ArgumentException("Index out of range.");
            return _data.GetValueOrDefault((row, column));
        }
        set
        {
            if (row < 0 || row >= Rows || column < 0 || column >= Columns) 
                throw new ArgumentException("Index out of range.");
            if ( value == 0.0f )
                {
                _data.Remove((row, column));
                return;
                }
                _data[(row, column)] =  value;
        }
    }

    public override Matrix Transpose()
    {
        SparseMatrix result = new SparseMatrix(Columns, Rows);
        foreach (var item in _data)
        {
           (int row, int column) = item.Key;
           result._data[(column, row)] = item.Value;
        }
        return result;
    }

    public override void Identity()
    {
        if (Rows != Columns)
            throw new InvalidOperationException("Matrix is not a square matrix");
        
        _data.Clear();

        for (int i = 0; i < Rows; i++)
            _data[(i, i)] = 1.0f;
    }

    public override float Norm()
    {
        float result = 0.0f;
        
        foreach (float val in _data.Values)
            result += val * val;
        
        return MathF.Sqrt(result);
    }
    protected override Matrix GetInstance(int rows, int cols)
    {
        return  new SparseMatrix(rows, cols);
    }

   

    
}