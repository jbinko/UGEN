// MIT License

// Copyright (c) 2020 Jiri Binko

// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:

// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.

// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System;
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
