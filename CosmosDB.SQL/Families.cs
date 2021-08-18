namespace CosmosDB.SQL
{

    public class FamilyExtended: Family
    {
        public string gender { get; set; }
    }

    public class Family
    {
        public string firstname { get; set; }
        
        public string familyname { get; set; }
        public Child[] children { get; set; }
        public string id { get; set; }
        public string _rid { get; set; }
        public string _self { get; set; }
        public string _etag { get; set; }
        public string _attachments { get; set; }
        public int _ts { get; set; }
    }

    public class Child
    {
        public string name { get; set; }
        public int age { get; set; }
    }

}
