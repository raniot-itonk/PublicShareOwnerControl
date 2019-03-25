using System;

namespace PublicShareOwnerControl.DB
{
    public class Shareholder
    {
        public long Id { get; set; }
        public Guid ShareholderId { get; set; }
        public int Amount { get; set; }
    }
}