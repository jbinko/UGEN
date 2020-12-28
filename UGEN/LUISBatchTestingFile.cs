﻿using System;
using System.Linq;
using System.Text.Json;
using System.Collections.Generic;

namespace UGEN
{
    internal sealed class JSONIntent
    {
        public string text { get; set; }
        public string intent { get; set; }
        public JSONIntentEntity[] entities { get; set; }
    }

    internal sealed class JSONIntentEntity
    {
        public string entity { get; set; }
        public int startPos { get; set; }
        public int endPos { get; set; }
    }

    internal sealed class LUISBatchTestingFile
    {
        public static string Create(List<CachedRule> generated, string fileName)
        {
            var json = Create(generated);
            if (String.IsNullOrWhiteSpace(fileName))
                System.Console.Write(json);
            else
                System.IO.File.WriteAllText(fileName, json);
            return json;
        }

        public static string Create(List<CachedRule> generated)
        {
            var intents = from x in generated
                          where x.Rule.Type == PatternRuleType.Intent
                          select x;

            var intentList = new List<JSONIntent>();

            foreach (var r in intents)
            {
                foreach (var s in r.StringEntities)
                {
                    var entitiList = new List<JSONIntentEntity>();
                    foreach (var e in s.Entities)
                    {
                        entitiList.Add(new JSONIntentEntity {

                            entity = e.EntityName,
                            startPos = e.StartPos,
                            endPos = e.EndPos
                        });
                    }
                    
                    intentList.Add(new JSONIntent {
                        text = s.Text,
                        intent = r.Rule.ID,
                        entities = entitiList.ToArray()
                    });
                }
            }

            return JsonSerializer.Serialize<JSONIntent[]>(intentList.ToArray(), new JsonSerializerOptions { WriteIndented = true });
        }
    }
}
