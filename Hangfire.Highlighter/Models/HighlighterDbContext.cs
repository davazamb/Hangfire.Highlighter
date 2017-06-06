using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace Hangfire.Highlighter.Models
{
    public class HighlighterDbContext : DbContext
    {
        public HighlighterDbContext() : base("HighlighterDb")
        {
        }

        public DbSet<CodeSnippet> CodeSnippets { get; set; }
    }
}