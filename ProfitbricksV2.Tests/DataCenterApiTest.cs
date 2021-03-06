using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;
using Api;
using Client;
using Model;
using System.Collections.Generic;

namespace ProfitbricksV2.Tests
{
    [TestClass]
    public class DataCenterTest
    {
        Configuration configuration;
        DataCenterApi dcApi;
        static Datacenter datacenter;


        [TestMethod]
        public void DataCenterCreate()
        {
            Configure();
            datacenter = new Datacenter
            {
                Properties = new DatacenterProperties
                {
                    Name = ".Net V2 - Test " + DateTime.Now.ToShortTimeString(),
                    Description = "Unit test for .Net SDK PB REST V2" ,
                    Location = "us/lasdev"
                }
            };

            datacenter = dcApi.Create(datacenter);
            Assert.IsNotNull(datacenter);

        }


        [TestMethod]
        public void DataCenterGet()
        {
            Configure();
            var dc = dcApi.FindById(datacenter.Id, depth: 5);

            Assert.AreEqual(datacenter.Id, dc.Id);
        }
        [TestMethod]
        public void DataCenterList()
        {
            Configure();
            var list = dcApi.FindAll(depth: 5);

            Assert.IsTrue(list.Items.Count > 0);
        }

        [TestMethod]
        public void DataCenterUpdate()
        {
            Configure();
            var resp = dcApi.PartialUpdate(datacenter.Id, new DatacenterProperties { Name = datacenter.Properties.Name + " - updated" });

            Assert.AreNotEqual(datacenter.Properties.Name, resp.Properties.Name);
        }

        [TestMethod]
        public void DataCenterDelete()
        {
            Configure();
            var resp = dcApi.Delete(datacenter.Id);
        }

        private void Configure()
        {
            configuration = new Configuration
            {
                Username = "test@stackpointcloud.com",
                Password = "pwd",

            };
            dcApi = new DataCenterApi(configuration);
        }

    }
}
