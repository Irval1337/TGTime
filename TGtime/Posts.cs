using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TGtime
{
    class Post
    {
        public int Id { get; set; }
        public string Message { get; set; }
        public Attachment[] Attachments { get; set; }
        public string Date { get; set; }
    }

    class Attachment
    {
        public string Path { get; set; }
    }

    class Posts
    {
        public Post[] posts { get; set; }
    }
}
