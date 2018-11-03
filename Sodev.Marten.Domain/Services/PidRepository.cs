using Sodev.Marten.Base.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sodev.Marten.Domain.Helpers;
using Sodev.Marten.Domain.Services;

namespace Sodev.Marten.Domain.Services
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
