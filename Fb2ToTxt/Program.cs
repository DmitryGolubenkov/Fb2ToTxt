using System.IO.Compression;

namespace Fb2ToTxt;
public static class Program
{

    private const string tempDirectoryName = "tempdirectoryfb2totxt";
    [STAThread]
    public static void Main()
    {
        try
        {
            Console.WriteLine("Запуск диалога выбора файла.");
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Title = "Выбор книги FB2";
            dialog.Filter = "Книги FB2 (*.fb2)|*.fb2";

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                Console.WriteLine("Файл выбран, обработка файла.");
                var fb2Book = dialog.FileName;
                if (IOHelper.CheckSignature(dialog.FileName, 4, IOHelper.SignatureZip))
                {
                    Console.WriteLine("Файл является архивом. Требуется разархивировать его перед обработкой.");
                    if (Directory.Exists(tempDirectoryName))
                    {
                        Directory.Delete(tempDirectoryName, true);
                    }
                    Directory.CreateDirectory(tempDirectoryName);
                    ZipFile.ExtractToDirectory(dialog.FileName, tempDirectoryName);
                    foreach (var f in Directory.GetFiles(tempDirectoryName).Where(f => f.Contains(".fb2")))
                    {
                        fb2Book = f;
                    }
                }


                List<string> file = File.ReadAllLines(fb2Book).ToList();
                file = ProcessFileContents(file);
                Console.WriteLine("Обработка завершена, переход к сохранению файла.");

                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Filter = "Текстовый файл (*.txt)|*.txt";
                saveFileDialog.FileName = $"Filtered [{dialog.SafeFileName}].txt";
                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    File.WriteAllLines(saveFileDialog.FileName, file);
                    Console.WriteLine("Файл сохранен: " + saveFileDialog.FileName);
                    ExploreFile(saveFileDialog.FileName);
                }
                else
                {
                    Console.WriteLine("Сохранение файла отменено. Завершение работы программы.");
                }
            }
            else
            {
                Console.WriteLine("Файл не выбран, завершение программы.");
            }
        }
        catch (Exception ex)
        {
            GenerateCrashReport(ex);
        }
        finally
        {
            if (Directory.Exists(tempDirectoryName))
            {
                Directory.Delete(tempDirectoryName, true);
            }
        }
    }

    private static List<string> ProcessFileContents(List<string> file)
    {
        for (int i = 0; i < file.Count; i++)
        {
            while (file[i].Contains('<') && file[i].Contains('>'))
            {
                var a = file[i].IndexOf("<");
                var b = file[i].IndexOf(">") + 1;
                if (a < b)
                {
                    file[i] = file[i].Remove(a, b - a);
                }
                else
                {
                    Console.WriteLine($"В строке {i + 1} возможна ошибка в обработке, так как > следует перед <");
                }
            }
            if (file[i].Trim() == string.Empty)
            {
                file.RemoveAt(i);
                i--;
            }
        }

        return file;
    }

    private static void GenerateCrashReport(Exception ex)
    {
        var dir = Directory.CreateDirectory("crashreports");
        var crashFileName = dir.FullName + $"/crash-{DateTime.Now:yyyy-MM-dd HH-mm-ss}.txt";

        string crashReport = $"При выполнении программы возникла ошибка: {ex.Message}\n\n. " +
            $"Трассировка стека: {ex.StackTrace}\n\n";
        if (ex.InnerException is not null)
        {
            crashReport += $"Внутреннее исключение (при наличии): {ex.InnerException}";
        }

        File.WriteAllText(crashFileName, crashReport);

        Console.WriteLine($"При выполнении программы возникла ошибка: {ex.Message}. " +
            $"\nПодробная информация сохранена в файле {crashFileName}");
        Console.WriteLine("\nДля продолжения нажмите любую клавишу.");
        Console.ReadKey();
    }

    private static void ExploreFile(string filePath)
    {
        if (File.Exists(filePath))
        {
            filePath = Path.GetFullPath(filePath);
            System.Diagnostics.Process.Start("explorer.exe", string.Format("/select,\"{0}\"", filePath));
        }
    }
}
