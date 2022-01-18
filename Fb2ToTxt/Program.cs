namespace Fb2ToTxt;
public static class Program
{
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
                List<string> file = File.ReadAllLines(dialog.FileName).ToList();
                for (int i = 0; i < file.Count; i++)
                {
                    while (file[i].Contains("<") && file[i].Contains(">"))
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
                Console.WriteLine("Обработка завершена, переход к сохранению файла.");
                var dir = Directory.CreateDirectory("export");
                var fileName = dir.FullName + $"/filtered {dialog.SafeFileName}.txt";
                File.WriteAllLines(fileName, file);
                Console.WriteLine("Файл сохранен: " + fileName);
                ExploreFile(fileName);
            }
            else
            {
                Console.WriteLine("Файл не выбран, завершение программы.");
            }
        }
        catch (Exception ex)
        {
            var dir = Directory.CreateDirectory("crashreports");
            var crashFileName = dir.FullName + $"/crash-{DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss")}.txt";

            string crashReport = $"При выполнении программы возникла ошибка: {ex.Message}\n\n. " +
                $"Трассировка стека: {ex.StackTrace}\n\n";
            if (ex.InnerException is not null)
            {
                crashReport+=$"Внутреннее исключение (при наличии): {ex.InnerException}";
            }

            File.WriteAllText(crashFileName, crashReport);

            Console.WriteLine($"При выполнении программы возникла ошибка: {ex.Message}. " +
                $"\nПодробная информация сохранена в файле {crashFileName}");
            Console.WriteLine("\nДля продолжения нажмите любую клавишу.");
            Console.ReadKey();
        }
    }

    public static bool ExploreFile(string filePath)
    {
        if (!File.Exists(filePath))
        {
            return false;
        }
        //Clean up file path so it can be navigated OK
        filePath = Path.GetFullPath(filePath);
        System.Diagnostics.Process.Start("explorer.exe", string.Format("/select,\"{0}\"", filePath));
        return true;
    }
}

