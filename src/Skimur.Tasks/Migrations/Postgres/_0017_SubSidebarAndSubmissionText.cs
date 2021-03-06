﻿using ServiceStack.OrmLite.Dapper;
using Skimur.Data;
using Skimur.Postgres.Migrations;

namespace Skimur.Tasks.Migrations.Postgres
{
    public class _0017_SubSidebarAndSubmissionText : Migration
    {
        public _0017_SubSidebarAndSubmissionText() : base(MigrationType.Schema, 17)
        {

        }
        
        public override void Execute(IDbConnectionProvider conn)
        {
            conn.Perform(x =>
            {
                x.Execute("ALTER TABLE subs ADD COLUMN sidebar_text_formatted text NULL;");
                x.Execute("ALTER TABLE subs ADD COLUMN submission_text text NULL;");
                x.Execute("ALTER TABLE subs ADD COLUMN submission_text_formatted text NULL;");
            });
        }

        public override string GetDescription()
        {
            return "Add fields to the subs table for submission text and sidebar text.";
        }
    }
}
