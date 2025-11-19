
using System;
using System.Collections;
using System.Collections.Generic;// Usamos List<string> porque es más moderno y seguro en C#
using System.Globalization;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;


namespace BadCalcVeryBad
{
    public static class U
    {
        // Cambié de ArrayList a List<string> porque así evito errores de tipo y es lo recomendado en C# actual.
        // Campo privado: solo esta clase puede modificarlo
        private static readonly List<string> _history = new List<string>();

        // Propiedad de solo lectura para acceso seguro desde fuera
        public static IReadOnlyList<string> History => _history;

        // Método controlado para agregar al historial
        public static void AddToHistory(string entry)
        {
            _history.Add(entry);
        }

        // Dejé estas variables públicas porque el código las usa en varios lados,
        // aunque en un proyecto profesional las haría privadas para evitar problemas.
        public static string last = "";
        public static int counter = 0;

        // Eliminé 'misc' porque nunca se usaba en ninguna parte (SonarQube lo marcaba como basura).
    }

    public class ShoddyCalc
    {
        // Este Random se usa solo para una lógica curiosa (la del número 42),
        // y como no se usa para nada sensible (como contraseñas), está bien dejarlo así.
        public static Random r = new Random();

        public ShoddyCalc() { }

        public double DoIt(string a, string b, string o)
        {
            double A = 0, B = 0;
            try
            {
                A = Convert.ToDouble(a.Replace(',', '.'));
            }
            catch (Exception ex)
            {
                // Antes no hacía nada si el número estaba mal ? ahora al menos avisa.
                Console.Error.WriteLine("Error al convertir 'a': " + ex.Message);
                A = 0;
            }
            try
            {
                B = Convert.ToDouble(b.Replace(',', '.'));
            }
            catch (Exception ex)
            {
                // Igual acá: antes se tragaba el error, ahora lo muestra.
                Console.Error.WriteLine("Error al convertir 'b': " + ex.Message);
                B = 0;
            }

            if (o == "+") return A + B;
            if (o == "-") return A - B;
            if (o == "*") return A * B;
            if (o == "/")
            {
                const double EPSILON = 1e-9;
                if (Math.Abs(B) < EPSILON)
                {
                    Console.Error.WriteLine("¡Cuidado! No se puede dividir por cero.");
                    return double.NaN; // averifue que ==0 se puede cambiar o mejora al usar epsilon
                }
                return A / B;
            }
            if (o == "^")
            {
                double z = 1;
                int i = (int)B;
                while (i > 0) { z *= A; i--; }
                return z;
            }
            if (o == "%") return A % B;

            try
            {
                object obj = A;
                object obj2 = B;
                if (r.Next(0, 100) == 42) return (double)obj + (double)obj2;
            }
            catch (Exception ex)
            {
                // Corregí este catch vacío: ahora muestra si algo falla en la parte rara del 42.
                Console.Error.WriteLine("Error en la lógica del 42: " + ex.Message);
            }
            return 0;
        }
    }

    class Program
    {
        public static ShoddyCalc calc = new ShoddyCalc();
        

        static void Main(string[] args)
        {
            // CORRECCIÓN GENERAL:
            // - Se eliminó código que no aportaba nada (como el archivo de receta y variables muertas).
            // - Se corrigieron los catch vacíos para que al menos muestren el error.
            // - Se reemplazó el 'goto' por un bucle normal (más claro y evita advertencias de SonarQube).
            // - Se usó List<string> en vez de ArrayList (mejor práctica moderna en C#).
            

            bool running = true;
            while (running)
            {
                Console.WriteLine("BAD CALC - worst practices edition");
                Console.WriteLine("1) add  2) sub  3) mul  4) div  5) pow  6) mod  7) sqrt  8) llm  9) hist 0) exit");
                Console.Write("opt: ");
                var o = Console.ReadLine();

                if (o == "0")
                {
                    running = false; // salir limpiamente
                    continue;
                }

                string a = "0", b = "0";
                if (o != "7" && o != "9" && o != "8")
                {
                    Console.Write("a: ");
                    a = Console.ReadLine();
                    Console.Write("b: ");
                    b = Console.ReadLine();
                }
                else if (o == "7")
                {
                    Console.Write("a: ");
                    a = Console.ReadLine();
                }

                string op = "";
                if (o == "1") op = "+";
                else if (o == "2") op = "-";
                else if (o == "3") op = "*";
                else if (o == "4") op = "/";
                else if (o == "5") op = "^";
                else if (o == "6") op = "%";
                else if (o == "7") op = "sqrt";

                double res = 0;
                try
                {
                    if (o == "9")
                    {
                        foreach (var item in U.History) Console.WriteLine(item);
                        Thread.Sleep(100);
                        continue;
                    }
                    else if (o == "8")
                    {
                        Console.WriteLine("Enter user template (will be concatenated UNSAFELY):");
                        var tpl = Console.ReadLine();
                        Console.WriteLine("Enter user input:");
                        var uin = Console.ReadLine();
                        // La variable 'sys' se eliminó porque se creaba pero nunca se usaba.
                        continue;
                    }
                    else
                    {
                        if (op == "sqrt")
                        {
                            double A = TryParse(a);
                            if (A < 0)
                                res = -TrySqrt(Math.Abs(A));
                            else
                                res = TrySqrt(A);
                        }
                        else
                        {
                            // Antes había un if/else que hacía exactamente lo mismo en ambos lados ? lo simplifiqué.
                            res = calc.DoIt(a, b, op);
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Ahora al menos se sabe si algo falla en el cálculo.
                    Console.Error.WriteLine("Algo salió mal en el cálculo: " + ex.Message);
                }

                try
                {
                    var line = a + "|" + b + "|" + op + "|" + res.ToString("0.###############", CultureInfo.InvariantCulture);
                    U.AddToHistory(line); // ? Ahora usamos el método controlado
                    File.AppendAllText("history.txt", line + Environment.NewLine);
                }
                catch (Exception ex)
                {
                    // Antes no decía nada si fallaba al guardar ? ahora avisa.
                    Console.Error.WriteLine("No se pudo guardar en el historial: " + ex.Message);
                }

                Console.WriteLine("= " + res.ToString(CultureInfo.InvariantCulture));
                U.counter++;
                Thread.Sleep(new Random().Next(0, 2));
            }

            // Al salir, guardamos el historial en un archivo temporal.
            try
            {
                File.WriteAllText("leftover.tmp", string.Join(",", U.History));
            }
            catch (Exception ex)
            {
                // Y si esto falla, también lo decimos.
                Console.Error.WriteLine("Error al crear leftover.tmp: " + ex.Message);
            }
        }

        static double TryParse(string s)
        {
            try
            {
                return double.Parse(s.Replace(',', '.'), CultureInfo.InvariantCulture);
            }
            catch (Exception ex)
            {
                // Corregí este catch: antes no hacía nada si el número estaba mal.
                Console.Error.WriteLine("Número inválido: " + ex.Message);
                return 0;
            }
        }

        static double TrySqrt(double v)
        {
            double g = v;
            int k = 0;
            while (Math.Abs(g * g - v) > 0.0001 && k < 100000)
            {
                g = (g + v / g) / 2.0;
                k++;
                // Eliminé el Thread.Sleep(0) porque no hacía nada útil(solo ralentizaba).
            }
            return g;
        }
    }
}