using MathSite.Models;
using Newtonsoft.Json;
using System.Collections.Specialized;
using System.Net;
using System.Text;
namespace MathSite.Functions
{
    public class UploadPictures
    {
        private TasksContext DataBase;

        public UploadPictures(TasksContext context)
        {
            DataBase = context;   
        }
        
        public void Upload(int TaskId, string Images)
        {
            string[] ImageList = Images.Split("|image|");
            foreach (string image in ImageList)
            {
                if (image.Length != 0)
                {
                    UploadToImgbb(TaskId, image);
                }
            }
        }

        private void UploadToImgbb(int TaskId, string image)
        {
            byte[] Response;
            string json = "";
            AzureSecretKey AzureSecretKey = new AzureSecretKey();

            using (var WebClient = new WebClient())
            {
                NameValueCollection parameters = new NameValueCollection();
                parameters.Add("key", AzureSecretKey.TakeSecretKey("Imgbb", "549917b67adf4d8fa16ff1c3b8b02527"));
                parameters.Add("image", image);
                try
                {
                    Response = WebClient.UploadValues("https://api.imgbb.com/1/upload", "POST", parameters);
                    json = Encoding.Default.GetString(Response);
                }
                catch { }
            }
            json.Root ResponseJson = JsonConvert.DeserializeObject<json.Root>(json);
            if (ResponseJson.data.url != null)
            {
                ReferenceToBase(ResponseJson.data.url, ResponseJson.data.delete_url, TaskId);
            }
        }

        private void ReferenceToBase(string Url, string DeleteUrl, int TaskId)
        {
            DataBase.PicturesRef.Add(new PictureRefModel() { Reference = Url, TaskId = TaskId, DeleteReference = DeleteUrl });
            DataBase.SaveChanges();
        }
    }
}
