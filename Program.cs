using System;
using System.Runtime.InteropServices;

class Program
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct FORM_INFO_1
    {
        public uint Flags;
        public IntPtr pName; // Ponteiro para a string nome do tamanho de papel
        public SIZEL Size;   // Tamanho do papel em décimos de milímetro
        public RECT ImageableArea; // Área imprimível
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SIZEL
    {
        public int cx;
        public int cy;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct RECT
    {
        public int left;
        public int top;
        public int right;
        public int bottom;
    }

    [DllImport("winspool.drv", CharSet = CharSet.Unicode, SetLastError = true)]
    public static extern bool OpenPrinter(string pPrinterName, out IntPtr phPrinter, IntPtr pDefault);

    [DllImport("winspool.drv", SetLastError = true)]
    public static extern bool AddForm(IntPtr hPrinter, uint Level, ref FORM_INFO_1 pForm);

    [DllImport("winspool.drv", SetLastError = true)]
    public static extern bool ClosePrinter(IntPtr hPrinter);

    static void Main()
    {
        string printerName = "CutePDF Writer"; // Nome da impressora
        string paperName = "1YAPI_Created_Paper";   // Nome do tamanho de papel
        int widthInMM = 297000;                  // Largura em décimos de milímetro (210mm)
        int heightInMM = 420000;                 // Altura em décimos de milímetro (297mm)

        IntPtr printerHandle;
        if (OpenPrinter(printerName, out printerHandle, IntPtr.Zero))
        {
            try
            {
                // Alocar memória para o nome do papel (com tamanho fixo)
                IntPtr pNamePtr = Marshal.StringToHGlobalUni(paperName);

                FORM_INFO_1 formInfo = new FORM_INFO_1
                {
                    Flags = 0,
                    pName = pNamePtr, // Usando o ponteiro para a string
                    Size = new SIZEL { cx = widthInMM, cy = heightInMM },
                    ImageableArea = new RECT { left = 0, top = 0, right = widthInMM, bottom = heightInMM }
                };

                // Adicionando o formulário
                if (AddForm(printerHandle, 1, ref formInfo))
                {
                    Console.WriteLine($"Custom paper size '{paperName}' added successfully.");
                }
                else
                {
                    // Se falhar, obtenha o código de erro e imprima-o
                    int errorCode = Marshal.GetLastWin32Error();
                    Console.WriteLine($"Failed to add custom paper size. Error Code: {errorCode}");
                }

                // Liberando a memória alocada para a string
                Marshal.FreeHGlobal(pNamePtr);
            }
            finally
            {
                ClosePrinter(printerHandle);
            }
        }
        else
        {
            // Se falhar ao abrir a impressora, obtenha o código de erro
            int errorCode = Marshal.GetLastWin32Error();
            Console.WriteLine($"Failed to open printer '{printerName}'. Error Code: {errorCode}");
        }
    }
}
