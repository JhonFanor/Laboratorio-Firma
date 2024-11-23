using System;
using System.IO;
using System.Security.Cryptography;

class Program
{
    static void Main()
    {
        var continuar = true;
        while(continuar)
        {
            Console.WriteLine("Que deseas hacer:");
            Console.WriteLine("1. Generar par de llaves.");
            Console.WriteLine("2. Firmar un archivo.");
            Console.WriteLine("3. Validar firma para un archvio.");
            Console.WriteLine("4. Salir.");
            string? opcion = Console.ReadLine()?.ToLower();

            switch(opcion) 
            {
            case "1":
                {
                    try
                    {
                        using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider(2048))
                        {
                            RSAParameters privateKey = rsa.ExportParameters(true);
                            RSAParameters publicKey = rsa.ExportParameters(false);

                            File.WriteAllText("privateKey.txt", ToXmlString(privateKey));
                            File.WriteAllText("publicKey.txt", ToXmlString(publicKey));
                            Console.WriteLine("Par de llaves generadas.");
                        }
                    }
                    catch (CryptographicException e)
                    {
                        Console.WriteLine($"Error de criptografía: {e.Message}");
                    }
                    break;
                }
            case "2":
                {
                    var continuarFirma = true;
                    Console.Clear();
                    while(continuarFirma)
                    {
                        Console.WriteLine("Que deseas firmar:");
                        Console.WriteLine("1. Subir archivo para firmar.");
                        Console.WriteLine("2. Generar un archivo txt para firmar.");
                        Console.WriteLine("3. Firmar todos los archivos.");
                        Console.WriteLine("4. Volver al menu anterior.");
                        string? opcionFirma = Console.ReadLine()?.ToLower();

                        switch(opcionFirma) 
                        {
                            case "1":
                                {
                                    try
                                    {
                                        string directorioActual = Directory.GetCurrentDirectory();

                                        string carpetaArchivos = Path.Combine(directorioActual, "Archivos");

                                        if (!Directory.Exists(carpetaArchivos))
                                        {
                                            Directory.CreateDirectory(carpetaArchivos);
                                        }

                                        Console.WriteLine("Arrastra y suelta el archivo aquí o escribe su ruta y presiona Enter:");
                                        string? rutaArchivo = Console.ReadLine()?.Trim('"', '\''); 
                                        if (File.Exists(rutaArchivo))
                                        {
                                            string nombreArchivo = Path.GetFileNameWithoutExtension(rutaArchivo);

                                            string destino = Path.Combine(carpetaArchivos, nombreArchivo);

                                            if (File.Exists(destino))
                                            {
                                                File.Delete(destino);
                                            }

                                            File.Move(rutaArchivo, destino);
                                            Console.WriteLine($"El archivo se ha movido a la carpeta 'Archivos': {destino}");

                                            string carpetaFirma = Path.Combine(directorioActual, "Firmas");

                                            if (!Directory.Exists(carpetaFirma))
                                            {
                                                Directory.CreateDirectory(carpetaFirma);
                                            }

                                            try
                                            {
                                                RSAParameters privateKeyParams = FromXmlString(File.ReadAllText("privateKey.txt"));

                                                using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider(2048))
                                                {
                                                    rsa.ImportParameters(privateKeyParams);

                                                    byte[] fileContent = File.ReadAllBytes(destino);

                                                    byte[] firma = rsa.SignData(fileContent, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

                                                    string firmaFilePath = Path.Combine(carpetaFirma, nombreArchivo + ".firma");
                                                    File.WriteAllBytes(firmaFilePath, firma);

                                                    Console.WriteLine($"Archivo firmado exitosamente. Firma guardada en: {firmaFilePath}");
                                                }
                                            }
                                            catch (CryptographicException e)
                                            {
                                                Console.WriteLine($"Error de criptografía: {e.Message}");
                                            }
                                        }
                                        else
                                        {
                                            Console.WriteLine("El archivo no existe. Por favor, verifica la ruta.");
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        Console.WriteLine($"Ocurrió un error: {ex.Message}");
                                    }
                                
                                    break;
                                }
                            case "2":
                                {
                                    try
                                    {
                                        string archivosPath = Path.Combine(Environment.CurrentDirectory, "Archivos");
                                        string firmasPath = Path.Combine(Environment.CurrentDirectory, "Firmas");
                                        Directory.CreateDirectory(archivosPath);
                                        Directory.CreateDirectory(firmasPath);

                                        RSAParameters privateKeyParams = FromXmlString(File.ReadAllText("privateKey.txt"));

                                        using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider(2048))
                                        {
                                            rsa.ImportParameters(privateKeyParams);

                                            string? nombreArchivo = null;
                                            string? mensaje = null;

                                            do
                                            {
                                                Console.WriteLine("Escribe el nombre del archivo (sin extensión):");
                                                nombreArchivo = Console.ReadLine()?.Trim();
                                                if (string.IsNullOrEmpty(nombreArchivo))
                                                {
                                                    Console.WriteLine("El nombre del archivo no puede estar vacío. Por favor, inténtalo de nuevo.");
                                                }
                                            } while (string.IsNullOrEmpty(nombreArchivo));

                                            do
                                            {
                                                Console.WriteLine("Escribe un mensaje:");
                                                mensaje = Console.ReadLine()?.Trim();
                                                if (string.IsNullOrEmpty(mensaje))
                                                {
                                                    Console.WriteLine("El mensaje no puede estar vacío. Por favor, inténtalo de nuevo.");
                                                }
                                            } while (string.IsNullOrEmpty(mensaje));

                                            string mensajePath = Path.Combine(archivosPath, $"{nombreArchivo}.txt");
                                            File.WriteAllText(mensajePath, mensaje);

                                            byte[] mensajeBytes = System.Text.Encoding.UTF8.GetBytes(mensaje);
                                            byte[] firma = rsa.SignData(mensajeBytes, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

                                            string firmaPath = Path.Combine(firmasPath, $"{nombreArchivo}.firma");
                                            File.WriteAllBytes(firmaPath, firma);

                                            Console.WriteLine($"El mensaje se ha guardado en: {mensajePath}");
                                            Console.WriteLine($"La firma se ha guardado en: {firmaPath}");
                                        }
                                    }
                                    catch (CryptographicException e)
                                    {
                                        Console.WriteLine($"Error de criptografía: {e.Message}");
                                    }
                                    catch (Exception e)
                                    {
                                        Console.WriteLine($"Error: {e.Message}");
                                    }
                                    break;
                                }
                            case "3":
                                {
                                    Console.Clear();
                                    break;
                                }
                            case "4":
                                {
                                    continuarFirma = false;
                                    Console.Clear();
                                    break;
                                }
                            default:
                                {
                                    Console.Clear();
                                    Console.WriteLine("Elija una opcion valida.");
                                    break;
                                }
                        }
                    }
                    break;
                }
            case "3":
                {
                    Console.WriteLine("Validar firmas:");
                    Console.WriteLine("1. Listar archivos.");
                    Console.WriteLine("2. Validar firma de un archivo.");
                    Console.WriteLine("3. Validar firma de todos los archivos.");
                    Console.WriteLine("4. Salir."); 
                    break;
                }
            case "4":
                {
                    Console.Clear();
                    Console.WriteLine("Adíos !!!!");
                    continuar = false;
                    break;
                }
            default:
                {
                    Console.Clear();
                    Console.WriteLine("Elija una opcion valida.");
                    break;
                }
            }
        }
    }

    static string ToXmlString(RSAParameters rsaParameters)
    {
        using (var sw = new System.IO.StringWriter())
        {
            var xs = new System.Xml.Serialization.XmlSerializer(typeof(RSAParameters));
            xs.Serialize(sw, rsaParameters);
            return sw.ToString();
        }
    }

    static RSAParameters FromXmlString(string xmlString)
    {
        using (var sr = new StringReader(xmlString))
        {
            var xs = new System.Xml.Serialization.XmlSerializer(typeof(RSAParameters));
            return (RSAParameters)xs.Deserialize(sr);
        }
    }
}