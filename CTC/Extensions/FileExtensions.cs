using System.Text.RegularExpressions;

namespace CTC.Extensions
{
    public static class FileExtensions
    {
        public static string ConvertFileToString(IFormFile file, IWebHostEnvironment webHostEnvironment)
        {
            string uploadsFolder = Path.Combine(webHostEnvironment.WebRootPath, "File");

            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(file.FileName);
            string filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                file.CopyTo(fileStream);
            }

            return "/File/" + uniqueFileName;
        }
        public static string ConvertImageToString(IFormFile imageFile, IWebHostEnvironment webHostEnvironment)
        {
            if (imageFile == null || imageFile.Length == 0)
            {
                return null; 
            }
            string uploadsFolder = Path.Combine(webHostEnvironment.WebRootPath, "Pic");

            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }
            string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(imageFile.FileName);
            string filePath = Path.Combine(uploadsFolder, uniqueFileName);
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                imageFile.CopyTo(fileStream); 
            }
            return uniqueFileName;
        }
        public static void DeleteFileFromFileFolder(string url, IWebHostEnvironment webHostEnvironment)
        {
            string uniqueUpload = Path.Combine(webHostEnvironment.WebRootPath, "File");
            string filePath = Path.Combine(uniqueUpload, url);
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }
        public static string TypeOfFile(string FileName)
        {
            string type = null;
            string regex = @"^.+.(jpg|png|jpeg|ppt|pptx|pps|ppsx|doc|docx|pdf)$";
            if (Regex.IsMatch(FileName.ToLower(), regex))
            {
                int indexDot = FileName.LastIndexOf(".");
                type = FileName.Substring(indexDot);
            }
            return type;


        }


        public static string ConvertVideoToString(IFormFile videoFile ,IWebHostEnvironment webHostEnvironment)
        {
            if (videoFile == null)
            {
                return null;    
            }
            string UploadsFolder = Path.Combine(webHostEnvironment.WebRootPath, "Video");
            if(!Directory.Exists(UploadsFolder))
            {
                Directory.CreateDirectory(UploadsFolder);
            }
            string uniqueFileName=Guid.NewGuid().ToString() + "_" + Path.GetFileName(videoFile.FileName);
            string filePath = Path.Combine(UploadsFolder, uniqueFileName);
            using(var fileStream = new FileStream(filePath,FileMode.Create))
            {
                videoFile.CopyTo(fileStream);
            }
            return "/Video/" + uniqueFileName;

        }
   
    }
}
