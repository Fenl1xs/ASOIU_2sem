using System;

int[,] matrixCOO = {
    { 1, 0, 0, 0, 0, 1 },
    { 0, 2, 0, 0, 0, 0 },
    { 3, 0, 0, 0, 0, 0 },
    { 0, 0, 0, 0, 0, 4 },
    { 0, 0, 5, 0, 0, 0 }
};

int[,] matrixLIL = {
    { 0, 1, 0, 2, 0, 0 },
    { 0, 0, 0, 0, 0, 3 },
    { 4, 0, 5, 0, 0, 0 },
    { 0, 0, 0, 6, 0, 0 }
};

int[,] matrixCSR = {
    { 8, 0, 2, 0, 0 },
    { 0, 0, 5, 0, 0 },
    { 0, 0, 0, 0, 0 },
    { 0, 0, 0, 0, 0 },
    { 0, 0, 7, 1, 2 },
    { 0, 0, 0, 0, 0 },
    { 0, 0, 0, 9, 0 }
};

PrintSeparator("задание 1: coo - координатный формат хранения");

int rCOO = matrixCOO.GetLength(0);
int cCOO = matrixCOO.GetLength(1);
int nCOO = CountNonZero(matrixCOO);

Console.WriteLine("исходная плотная матрица (рисунок 1, 5x6):");
PrintMatrix(matrixCOO);

Console.WriteLine("\n--- анализ эффективности coo ---");
Console.WriteLine($" размерность матрицы: {rCOO} x {cCOO} = {rCOO * cCOO} ячеек (плотное хранение)");
Console.WriteLine($" ненулевых элементов (n): {nCOO}");
Console.WriteLine($" coo хранение (n*3): {nCOO} * 3 = {nCOO * 3} ячеек");
Console.WriteLine($" порог эффективности r*c/3: {rCOO} * {cCOO} / 3 = {(double)(rCOO * cCOO) / 3:F2}");
Console.WriteLine($" условие (n*3 < r*c): {nCOO * 3} < {rCOO * cCOO} => iscooeffective = {isCOOEffective(matrixCOO)}");

Console.WriteLine("\n--- преобразование dense -> coo ---");
DenseToCOO(matrixCOO, out int[] rowCOO, out int[] colCOO, out int[] dataCOO);
PrintArray("row", rowCOO);
PrintArray("column", colCOO);
PrintArray("data", dataCOO);

Console.WriteLine("\n--- преобразование coo -> dense ---");
int[,] restoredCOO = COOToDense(rowCOO, colCOO, dataCOO, rCOO, cCOO);
PrintMatrix(restoredCOO);
Console.WriteLine($" матрица восстановлена верно: {MatricesEqual(matrixCOO, restoredCOO)}");

PrintSeparator("задание 2: lil - хранение в форме связных списков");

int rLIL = matrixLIL.GetLength(0);
int cLIL = matrixLIL.GetLength(1);
int nLIL = CountNonZero(matrixLIL);

Console.WriteLine("исходная плотная матрица (рисунок 2, 4x6):");
PrintMatrix(matrixLIL);

Console.WriteLine("\n--- анализ эффективности lil ---");
Console.WriteLine($" размерность матрицы: {rLIL} x {cLIL} = {rLIL * cLIL} ячеек (плотное хранение)");
Console.WriteLine($" ненулевых элементов (n): {nLIL}");
Console.WriteLine($" lil хранение (n*2): {nLIL} * 2 = {nLIL * 2} ячеек");
Console.WriteLine($" порог эффективности r*c/2: {rLIL} * {cLIL} / 2 = {(double)(rLIL * cLIL) / 2:F2}");
Console.WriteLine($" условие (n*2 < r*c): {nLIL * 2} < {rLIL * cLIL} => islileffective = {isLILEffective(matrixLIL)}");

Console.WriteLine("\n--- преобразование dense -> lil ---");
DenseToLIL(matrixLIL, out int[][] rowsLIL, out int[][] dataLIL);
Console.WriteLine(" rows[i] - индексы столбцов ненулевых элементов строки i:");
PrintJaggedArray("rows", rowsLIL);
Console.WriteLine(" data[i] - значения ненулевых элементов строки i:");
PrintJaggedArray("data", dataLIL);

Console.WriteLine("\n--- преобразование lil -> dense ---");
int[,] restoredLIL = LILToDense(rowsLIL, dataLIL, rLIL, cLIL);
PrintMatrix(restoredLIL);
Console.WriteLine($" матрица восстановлена верно: {MatricesEqual(matrixLIL, restoredLIL)}");

PrintSeparator("задание 3: csr - компактное хранение разреженных матриц");

int rCSR = matrixCSR.GetLength(0);
int cCSR = matrixCSR.GetLength(1);
int nCSR = CountNonZero(matrixCSR);

Console.WriteLine("исходная плотная матрица (рисунок 3, 7x5):");
PrintMatrix(matrixCSR);

Console.WriteLine("\n--- анализ эффективности csr ---");
Console.WriteLine($" размерность матрицы: {rCSR} x {cCSR} = {rCSR * cCSR} ячеек (плотное хранение)");
Console.WriteLine($" ненулевых элементов (n): {nCSR}");
Console.WriteLine($" csr хранение (2*n + r + 1): 2*{nCSR} + {rCSR} + 1 = {2 * nCSR + rCSR + 1} ячеек");
Console.WriteLine($" порог: (r*c - r - 1) / 2: ({rCSR} * {cCSR} - {rCSR} - 1) / 2 = {(double)(rCSR * cCSR - rCSR - 1) / 2:F2}");
Console.WriteLine($" условие (2*n+r+1 < r*c): {2 * nCSR + rCSR + 1} < {rCSR * cCSR} => iscsreffective = {isCSREffective(matrixCSR)}");

Console.WriteLine("\n--- преобразование dense -> csr ---");
DenseToCSR(matrixCSR, out int[] dataCSR, out int[] indicesCSR, out int[] ipCSR);
PrintArray("data", dataCSR);
PrintArray("indices", indicesCSR);
PrintArray("indexpointers", ipCSR);

Console.WriteLine("\n--- пошаговое декодирование строк из csr ---");
for (int i = 0; i < rCSR; i++)
{
    int start = ipCSR[i];
    int end = ipCSR[i + 1];
    Console.Write($" row {i}: ip[{i}]={start}, ip[{i + 1}]={end} => ");
    if (start == end)
    {
        Console.WriteLine("(пустая строка)");
    }
    else
    {
        for (int k = start; k < end; k++)
        {
            Console.Write($"[{i},{indicesCSR[k]}]={dataCSR[k]} ");
        }
        Console.WriteLine();
    }
}

Console.WriteLine("\n--- преобразование csr -> dense ---");
int[,] restoredCSR = CSRToDense(dataCSR, indicesCSR, ipCSR, rCSR, cCSR);
PrintMatrix(restoredCSR);
Console.WriteLine($" матрица восстановлена верно: {MatricesEqual(matrixCSR, restoredCSR)}");

Console.WriteLine("\n=== программа завершена ===");

static void PrintSeparator(string title)
{
    string line = new string('=', 62);
    Console.WriteLine($"\n{line}");
    Console.WriteLine($" {title}");
    Console.WriteLine($"{line}\n");
}

static void PrintMatrix(int[,] m)
{
    int rows = m.GetLength(0);
    int cols = m.GetLength(1);
    for (int i = 0; i < rows; i++)
    {
        for (int j = 0; j < cols; j++)
        {
            Console.Write($"{m[i, j],4}");
        }
        Console.WriteLine();
    }
}

static void PrintArray(string name, int[] arr)
{
    Console.Write($"{name}: [");
    for (int i = 0; i < arr.Length; i++)
    {
        Console.Write($"{arr[i]}");
        if (i < arr.Length - 1) Console.Write(", ");
    }
    Console.WriteLine("]");
}

static void PrintJaggedArray(string name, int[][] arr)
{
    for (int i = 0; i < arr.Length; i++)
    {
        Console.Write($"  {name}[{i}]: [");
        if (arr[i] != null)
        {
            for (int j = 0; j < arr[i].Length; j++)
            {
                Console.Write($"{arr[i][j]}");
                if (j < arr[i].Length - 1) Console.Write(", ");
            }
        }
        Console.WriteLine("]");
    }
}

static int CountNonZero(int[,] m)
{
    int count = 0;
    for (int i = 0; i < m.GetLength(0); i++)
        for (int j = 0; j < m.GetLength(1); j++)
            if (m[i, j] != 0)
                count++;
    return count;
}

static bool MatricesEqual(int[,] a, int[,] b)
{
    if (a.GetLength(0) != b.GetLength(0) || a.GetLength(1) != b.GetLength(1))
        return false;
    for (int i = 0; i < a.GetLength(0); i++)
        for (int j = 0; j < a.GetLength(1); j++)
            if (a[i, j] != b[i, j])
                return false;
    return true;
}

static void DenseToCOO(int[,] dense, out int[] row, out int[] col, out int[] data)
{
    int rows = dense.GetLength(0);
    int cols = dense.GetLength(1);
    int count = CountNonZero(dense);
    row = new int[count];
    col = new int[count];
    data = new int[count];
    int idx = 0;
    for (int i = 0; i < rows; i++)
        for (int j = 0; j < cols; j++)
            if (dense[i, j] != 0)
            {
                row[idx] = i;
                col[idx] = j;
                data[idx] = dense[i, j];
                idx++;
            }
}

static int[,] COOToDense(int[] row, int[] col, int[] data, int numRows, int numCols)
{
    int[,] dense = new int[numRows, numCols];
    for (int k = 0; k < data.Length; k++)
        dense[row[k], col[k]] = data[k];
    return dense;
}

static bool isCOOEffective(int[,] dense)
{
    int r = dense.GetLength(0);
    int c = dense.GetLength(1);
    int n = CountNonZero(dense);
    return n * 3 < r * c;
}

static void DenseToLIL(int[,] dense, out int[][] rows, out int[][] data)
{
    int numRows = dense.GetLength(0);
    int numCols = dense.GetLength(1);
    rows = new int[numRows][];
    data = new int[numRows][];
    for (int i = 0; i < numRows; i++)
    {
        int count = 0;
        for (int j = 0; j < numCols; j++)
            if (dense[i, j] != 0)
                count++;
        rows[i] = new int[count];
        data[i] = new int[count];
        int idx = 0;
        for (int j = 0; j < numCols; j++)
            if (dense[i, j] != 0)
            {
                rows[i][idx] = j;
                data[i][idx] = dense[i, j];
                idx++;
            }
    }
}

static int[,] LILToDense(int[][] rows, int[][] data, int numRows, int numCols)
{
    int[,] dense = new int[numRows, numCols];
    for (int i = 0; i < numRows; i++)
        for (int k = 0; k < rows[i].Length; k++)
            dense[i, rows[i][k]] = data[i][k];
    return dense;
}

static bool isLILEffective(int[,] dense)
{
    int r = dense.GetLength(0);
    int c = dense.GetLength(1);
    int n = CountNonZero(dense);
    return n * 2 < r * c;
}

static void DenseToCSR(int[,] dense, out int[] data, out int[] indices, out int[] indexPointers)
{
    int numRows = dense.GetLength(0);
    int numCols = dense.GetLength(1);
    int n = CountNonZero(dense);
    data = new int[n];
    indices = new int[n];
    indexPointers = new int[numRows + 1];
    int idx = 0;
    for (int i = 0; i < numRows; i++)
    {
        indexPointers[i] = idx;
        for (int j = 0; j < numCols; j++)
            if (dense[i, j] != 0)
            {
                data[idx] = dense[i, j];
                indices[idx] = j;
                idx++;
            }
    }
    indexPointers[numRows] = n;
}

static int[,] CSRToDense(int[] data, int[] indices, int[] indexPointers, int numRows, int numCols)
{
    int[,] dense = new int[numRows, numCols];
    for (int i = 0; i < numRows; i++)
    {
        int start = indexPointers[i];
        int end = indexPointers[i + 1];
        for (int k = start; k < end; k++)
            dense[i, indices[k]] = data[k];
    }
    return dense;
}

static bool isCSREffective(int[,] dense)
{
    int r = dense.GetLength(0);
    int c = dense.GetLength(1);
    int n = CountNonZero(dense);
    return 2 * n + r + 1 < r * c;
}