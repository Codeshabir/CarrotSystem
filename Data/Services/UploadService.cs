using iText.IO.Image;
using iText.Kernel.Pdf;
using iText.Layout;
using File = System.IO.File;

namespace CarrotSystem.Services
{
    public class UploadService
    {
        public static string UploadFile(IFormFile file, string fileType)
        {
            if (file != null)
            {
                var allowedExtensions = new[] { "" };
                var imgExt = new[] { ".exe" };
                string uploadPath = "";

                if (fileType.Equals("Image"))
                {
                    allowedExtensions = new[] {
                        ".png", ".jpg", ".jpeg", ".gif"
                    };

                    uploadPath = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads/images"));
                }
                else if (fileType.Equals("Video"))
                {
                    allowedExtensions = new[] {
                        ".mp4", ".mkv", ".avi", ".wmv"
                    };

                    uploadPath = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads/videos"));
                }
                else if (fileType.Equals("QRCode"))
                {
                    allowedExtensions = new[] {
                        ".png", ".jpg", ".jpeg", ".gif", ".pdf"
                    };

                    uploadPath = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads/qrcode"));
                }
                else if (fileType.Equals("Invoice"))
                {
                    allowedExtensions = new[] {
                        ".png", ".jpg", ".jpeg", ".gif", ".pdf", ".doc", ".docx", ".xls", ".xlsx"
                    };

                    uploadPath = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads/invoice"));
                }
                else
                {
                    uploadPath = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads/etc"));
                }

                var ext = Path.GetExtension(file.FileName);

                if (allowedExtensions.Contains(ext))
                {
                    string name = Path.GetFileNameWithoutExtension(Path.GetFileName(file.FileName));
                    string myfileName = Guid.NewGuid().ToString() + ext;

                    uploadPath = uploadPath + DateTime.Now.ToString("/yyyy/MM/");

                    string fullpath = Path.Combine(uploadPath, myfileName);

                    if (!Directory.Exists(uploadPath))
                        Directory.CreateDirectory(uploadPath);

                    if (File.Exists(fullpath))
                        myfileName = Guid.NewGuid().ToString() + ext;

                    fullpath = Path.Combine(uploadPath, myfileName);

                    // Check if the uploaded file is an image
                    if (imgExt.Contains(ext))
                    {
                        string pdfFilePath = Path.ChangeExtension(fullpath, "pdf");

                        byte[] imgFileBytes;

                        using (var imageStream = new FileStream(fullpath, FileMode.Create))
                        {
                            using (BinaryReader br = new BinaryReader(imageStream))
                            {
                                imgFileBytes = br.ReadBytes((int)imageStream.Length);
                            }

                            using (var pdfStream = new FileStream(pdfFilePath, FileMode.Create))
                            {
                                var writer = new PdfWriter(pdfStream);
                                var pdf = new PdfDocument(writer);
                                var document = new Document(pdf);

                                // Load the image
                                var imageData = ImageDataFactory.Create(fullpath);
                                var imgWidthInPoints = imageData.GetWidth();
                                var imgHeightInPoints = imageData.GetHeight();

                                // Create an iTextSharp Image object with specified dimensions
                                var img = new iText.Layout.Element.Image(imageData)
                                    .SetWidth(imgWidthInPoints)
                                    .SetHeight(imgHeightInPoints);

                                // Add the image to the PDF document at position (0, 0)
                                document.Add(img.SetFixedPosition(0, 0));

                                // Close the document
                                document.Close();
                            }
                        }

                        return pdfFilePath;
                    }
                    else
                    {
                        // Save the uploaded file as is (without conversion) to the specified path
                        using (FileStream stream = new FileStream(fullpath, FileMode.Create))
                        {
                            file.CopyTo(stream);
                        }

                        // Return the path to the saved file
                        return fullpath;
                    }
                }
                return null;
            }
            return null;
        }

        public static void DeleteFile(string fullpath)
        {
            try
            {
                if (String.IsNullOrEmpty(fullpath))
                    return;

                string paths = Path.GetFullPath(fullpath);
                FileInfo file = new FileInfo(paths);
                if (file.Exists)//check file exsit or not  
                {
                    file.Delete();
                }
                else
                {
                    return;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

    }
}
