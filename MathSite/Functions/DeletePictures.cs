using MathSite.Models;
using System;
using System.Linq;

namespace MathSite.Functions
{
    public class DeletePictures
    {
        private TasksContext DataBase;

        public DeletePictures(TasksContext context)
        {
            DataBase = context;
        } 

        public void Delete(string ImagesId)
        {
            string[] ImageIdList = ImagesId.Split("#");

            foreach (string Id in ImageIdList)
            {
                int ImageId = Convert.ToInt32(Id);
                PictureRefModel OldPicture = GetReference(ImageId);
                DataBase.PicturesRef.Remove(OldPicture);
            }
            DataBase.SaveChanges();
        }

        private PictureRefModel GetReference(int ImageId)
        {
            return DataBase.PicturesRef.Where(x => x.Id == ImageId).FirstOrDefault();
        }
    }
}
