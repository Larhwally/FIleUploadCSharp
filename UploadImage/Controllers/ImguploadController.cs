using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using System.Net.Http;
using System.Net.Http.Headers;
using UploadImage.Data;
using UploadImage.Model;

namespace UploadImage.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ImguploadController : ControllerBase
    {
        public static IHostingEnvironment _environment;
        public ImguploadController(IHostingEnvironment environment)
        {
            _environment = environment;
        }

        [HttpPost]
        public async Task<string> Post([FromForm][Bind("imgPath", "imgUniquePath", "uploadBy", "uploadDatetime")]  IFormFile file)
        {

            string fName = file.FileName;
            string uniqueName = Guid.NewGuid() + "" + "_" + fName;

            if (!file.ContentType.StartsWith("image/"))
            {
                //return BadRequest("File is not an image");
                return ("file is not an image");
            }
            if (!file.FileName.EndsWith("jpg") & !file.FileName.EndsWith("jpeg"))
            {
                //return BadRequest("Image is not in jpeg format");
                return ("Image is not in Jpeg format");
            }

            if (file.Length < 1024 * 1024 * 2)
            {
                string path = Path.Combine(_environment.ContentRootPath, "wwwroot/" + uniqueName);

                using (var stream = new FileStream(path, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }
                //Save image details to database
                EntityConnection con = new EntityConnection("tbl_images");
                Dictionary<string, string> param = new Dictionary<string, string>();
                param.Add("uploadDateTime", DateTime.Now.ToString());
                param.Add("imgPath", path);
                param.Add("uploadBy", "admin");
                param.Add("imgUniquePath", uniqueName);
                con.Insert(param);
                //if (param != null)
                //{
                //    con.Insert(param);
                //}
                //return Ok(uniqueName);
                string result = "{'status': true, 'data':" + EntityConnection.ToJson(uniqueName) + "}";
                return result;
            }
            else
            {
                //return BadRequest("file to large!");
                return ("file too large");
            }

        }


        [HttpGet("{uniquePath}")]
        public IActionResult Get(string uniquePath)
        {
            EntityConnection con = new EntityConnection("tbl_images");
            if (con.CheckImage(uniquePath) == true)
            {
                string path = Path.Combine(_environment.ContentRootPath, "wwwroot/" + uniquePath);
                using (var stream = new FileStream(path, FileMode.Open))
                {
                    return PhysicalFile(path, "image/jpg");
                }
            }
            else
            {
                return BadRequest("Image does not exist");
            }
        }




    }
}