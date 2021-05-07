﻿using DFC.ServiceTaxonomy.Neo4j.Queries;
using DFC.ServiceTaxonomy.Neo4j.Queries.Interfaces;
using Neo4j.Driver;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace DFC.Api.Lmi.Import.Models
{
    [ExcludeFromCodeCoverage]
    public class GenericCypherQueryModel : IQuery<IRecord>
    {
        public GenericCypherQueryModel(string query)
        {
            QueryToRun = query;
        }

        public string QueryToRun { get; set; }

        public Query Query
        {
            get
            {
                this.CheckIsValid();
                return new Query(QueryToRun);
            }
        }

        public List<string> ValidationErrors()
        {
            var validationErrors = new List<string>();

            if (string.IsNullOrEmpty(QueryToRun))
            {
                validationErrors.Add("No query specified to run.");
            }

            return validationErrors;
        }

        public IRecord ProcessRecord(IRecord record)
        {
            return record;
        }
    }
}
