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
                                            string nombreArchivo = Path.GetFileName(rutaArchivo);

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
                                    try
                                    {
                                        string archivosPath = Path.Combine(Environment.CurrentDirectory, "Archivos");
                                        string firmasPath = Path.Combine(Environment.CurrentDirectory, "Firmas");

                                        if (!Directory.Exists(archivosPath))
                                        {
                                            Console.WriteLine("La carpeta 'Archivos' no existe. Por favor, sube o crea archivos primero.");
                                            break;
                                        }

                                        Directory.CreateDirectory(firmasPath);

                                        RSAParameters privateKeyParams = FromXmlString(File.ReadAllText("privateKey.txt"));

                                        using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider(2048))
                                        {
                                            rsa.ImportParameters(privateKeyParams);

                                            string[] archivos = Directory.GetFiles(archivosPath);

                                            if (archivos.Length == 0)
                                            {
                                                Console.WriteLine("No hay archivos en la carpeta 'Archivos' para firmar.");
                                                break;
                                            }

                                            foreach (var archivo in archivos)
                                            {
                                                try
                                                {
                                                    string nombreArchivo = Path.GetFileName(archivo);

                                                    // Leer contenido del archivo
                                                    byte[] contenidoArchivo = File.ReadAllBytes(archivo);

                                                    // Generar firma
                                                    byte[] firma = rsa.SignData(contenidoArchivo, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

                                                    // Definir ruta de la firma
                                                    string firmaPath = Path.Combine(firmasPath, $"{nombreArchivo}.firma");

                                                    // Verificar si ya existe una firma y eliminarla
                                                    if (File.Exists(firmaPath))
                                                    {
                                                        File.Delete(firmaPath);
                                                        Console.WriteLine($"Firma existente para '{nombreArchivo}' eliminada.");
                                                    }

                                                    // Guardar la nueva firma
                                                    File.WriteAllBytes(firmaPath, firma);
                                                    Console.WriteLine($"Archivo '{nombreArchivo}' firmado exitosamente. Nueva firma guardada en: {firmaPath}");
                                                }
                                                catch (Exception ex)
                                                {
                                                    Console.WriteLine($"Error al firmar el archivo '{archivo}': {ex.Message}");
                                                }
                                            }
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
                    var continuarValidacion = true;
                    while (continuarValidacion)
                    {
                        Console.WriteLine("Validar firmas:");
                        Console.WriteLine("1. Listar archivos.");
                        Console.WriteLine("2. Validar firma de un archivo.");
                        Console.WriteLine("3. Validar firma de todos los archivos.");
                        Console.WriteLine("4. Volver al menú anterior.");
                        string? opcionValidar = Console.ReadLine()?.Trim();

                        switch (opcionValidar)
                        {
                            case "1":
                                {
                                    try
                                    {
                                        string archivosPath = Path.Combine(Environment.CurrentDirectory, "Archivos");

                                        if (!Directory.Exists(archivosPath))
                                        {
                                            Console.WriteLine("La carpeta 'Archivos' no existe.");
                                            break;
                                        }

                                        string[] archivos = Directory.GetFiles(archivosPath);

                                        if (archivos.Length == 0)
                                        {
                                            Console.WriteLine("No hay archivos en la carpeta 'Archivos'.");
                                        }
                                        else
                                        {
                                            Console.WriteLine("Archivos en la carpeta 'Archivos':");
                                            foreach (var archivo in archivos)
                                            {
                                                Console.WriteLine($"- {Path.GetFileName(archivo)}");
                                            }
                                        }
                                    }
                                    catch (Exception e)
                                    {
                                        Console.WriteLine($"Error: {e.Message}");
                                    }
                                    break;
                                }

                            case "2":
                                {
                                    try
                                    {
                                        string archivosPath = Path.Combine(Environment.CurrentDirectory, "Archivos");
                                        string firmasPath = Path.Combine(Environment.CurrentDirectory, "Firmas");

                                        Console.WriteLine("Escribe el nombre del archivo (con extensión):");
                                        string? nombreArchivo = Console.ReadLine()?.Trim();

                                        if (string.IsNullOrEmpty(nombreArchivo))
                                        {
                                            Console.WriteLine("El nombre del archivo no puede estar vacío.");
                                            break;
                                        }

                                        string archivoPath = Path.Combine(archivosPath, nombreArchivo);
                                        string firmaPath = Path.Combine(firmasPath, $"{nombreArchivo}.firma");

                                        if (!File.Exists(archivoPath))
                                        {
                                            Console.WriteLine($"El archivo '{archivoPath}' no existe.");
                                            break;
                                        }

                                        if (!File.Exists(firmaPath))
                                        {
                                            Console.WriteLine($"El archivo '{nombreArchivo}' no tiene firma.");
                                            break;
                                        }

                                        // Validar firma
                                        RSAParameters publicKeyParams = FromXmlString(File.ReadAllText("publicKey.txt"));

                                        using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider(2048))
                                        {
                                            rsa.ImportParameters(publicKeyParams);

                                            byte[] contenidoArchivo = File.ReadAllBytes(archivoPath);
                                            byte[] firmaArchivo = File.ReadAllBytes(firmaPath);

                                            bool esValido = rsa.VerifyData(contenidoArchivo, firmaArchivo, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

                                            if (esValido)
                                            {
                                                Console.WriteLine($"La firma del archivo '{nombreArchivo}' es válida.");
                                            }
                                            else
                                            {
                                                Console.WriteLine($"La firma del archivo '{nombreArchivo}' no es válida.");
                                            }
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
                                    try
                                    {
                                        string archivosPath = Path.Combine(Environment.CurrentDirectory, "Archivos");
                                        string firmasPath = Path.Combine(Environment.CurrentDirectory, "Firmas");

                                        if (!Directory.Exists(archivosPath))
                                        {
                                            Console.WriteLine("La carpeta 'Archivos' no existe.");
                                            break;
                                        }

                                        Directory.CreateDirectory(firmasPath);
                                        string[] archivos = Directory.GetFiles(archivosPath);

                                        if (archivos.Length == 0)
                                        {
                                            Console.WriteLine("No hay archivos en la carpeta 'Archivos'.");
                                            break;
                                        }

                                        RSAParameters publicKeyParams = FromXmlString(File.ReadAllText("publicKey.txt"));

                                        using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider(2048))
                                        {
                                            rsa.ImportParameters(publicKeyParams);

                                            foreach (var archivo in archivos)
                                            {
                                                string nombreArchivo = Path.GetFileName(archivo);
                                                string firmaPath = Path.Combine(firmasPath, $"{nombreArchivo}.firma");

                                                if (!File.Exists(firmaPath))
                                                {
                                                    Console.WriteLine($"El archivo '{nombreArchivo}' no tiene firma.");
                                                    continue;
                                                }

                                                try
                                                {
                                                    byte[] contenidoArchivo = File.ReadAllBytes(archivo);
                                                    byte[] firmaArchivo = File.ReadAllBytes(firmaPath);

                                                    bool esValido = rsa.VerifyData(contenidoArchivo, firmaArchivo, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

                                                    if (esValido)
                                                    {
                                                        Console.WriteLine($"La firma del archivo '{nombreArchivo}' es válida.");
                                                    }
                                                    else
                                                    {
                                                        Console.WriteLine($"La firma del archivo '{nombreArchivo}' no es válida.");
                                                    }
                                                }
                                                catch (Exception ex)
                                                {
                                                    Console.WriteLine($"Error al validar la firma del archivo '{nombreArchivo}': {ex.Message}");
                                                }
                                            }
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

                            case "4":
                                {
                                    continuarValidacion = false;
                                    Console.Clear();
                                    break;
                                }

                            default:
                                {
                                    Console.Clear();
                                    Console.WriteLine("Elija una opción válida.");
                                    break;
                                }
                        }
                    }
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