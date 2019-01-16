using System.Collections.Generic;
using System.IO;
using Sodev.Marten.Base.Helpers;
using Sodev.Marten.Base.Model;

namespace Sodev.Marten.Base.Services
{
    public class PidRepository : IPidRepository
    {
        //TODO add exception handler...
        public IEnumerable<PidParameters> GetAllPidsParameters()
        {
            var hardcodedPath = "pids.xml";//TODO get rid of it!
            var xmlString = OpenFile(hardcodedPath);
            return XmlSerializeHelper.DeSerializeObject<List<PidParameters>>(xmlString);
        }

        private string OpenFile(string path) => File.ReadAllText(path);
    }

}
