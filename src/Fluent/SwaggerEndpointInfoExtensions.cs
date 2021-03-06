﻿using Nancy.Metadata.Swagger.Core;
using Nancy.Metadata.Swagger.Model;
using NJsonSchema;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Nancy.Metadata.Swagger.Fluent
{
    public static class SwaggerEndpointInfoExtensions
    {
        public static SwaggerEndpointInfo WithResponseModel(this SwaggerEndpointInfo endpointInfo, string statusCode, Type modelType, string description = null)
        {
            if (endpointInfo.ResponseInfos == null)
            {
                endpointInfo.ResponseInfos = new Dictionary<string, SwaggerResponseInfo>();
            }

            endpointInfo.ResponseInfos[statusCode] = GenerateResponseInfo(description, modelType);

            return endpointInfo;
        }

        public static SwaggerEndpointInfo WithDefaultResponse(this SwaggerEndpointInfo endpointInfo, Type responseType, string description = "Default response") =>
         endpointInfo.WithResponseModel("200", responseType, description);

        public static SwaggerEndpointInfo WithResponse(this SwaggerEndpointInfo endpointInfo, string statusCode, string description)
        {
            if (endpointInfo.ResponseInfos == null)
            {
                endpointInfo.ResponseInfos = new Dictionary<string, SwaggerResponseInfo>();
            }

            endpointInfo.ResponseInfos[statusCode] = GenerateResponseInfo(description);

            return endpointInfo;
        }

        public static SwaggerEndpointInfo WithRequestParameter(this SwaggerEndpointInfo endpointInfo, string name,
            string type = "string", string format = null, bool required = true, string description = null, object defaultValue = null,
            long? maximum = null, bool? exclusiveMaximum = null, long? minimum = null, bool? exclusiveMinimum = null,
            long? maxLength = null, long? minLength = null, int? maxItems = null, int? minItems = null,
            string pattern = null, bool? uniqueItems = null, IEnumerable<string> enumValue = null, int? multipleOf = null,  string loc = "path", bool isArray = false)
        {
            if (endpointInfo.RequestParameters == null)
            {
                endpointInfo.RequestParameters = new List<SwaggerRequestParameter>();
            }

            var item = new Item();

            if (isArray)
            {
                item.Type = type;
                type = "array";
            }

            endpointInfo.RequestParameters.Add(new SwaggerRequestParameter
            {
                Required = required,
                Description = description,
                Format = format,
                In = loc,
                Name = name,
                Type = type,
                Item = item,
                Default = defaultValue,
                Maximum = maximum,
                ExclusiveMaximum = exclusiveMaximum,
                Minimum = minimum,
                ExclusiveMinimum = exclusiveMinimum,
                MaxLength = maxLength,
                MinLength = minLength,
                MaxItems = maxItems,
                MinItems = minItems,
                Pattern = pattern,
                UniqueItems = uniqueItems,
                Enum = enumValue,
                MultipleOf = multipleOf,
            });

            return endpointInfo;
        }

        public static SwaggerEndpointInfo WithRequestModel(this SwaggerEndpointInfo endpointInfo, Type requestType, string name = "body", string description = null, bool required = true, string loc = "body")
        {
            if (endpointInfo.RequestParameters == null)
            {
                endpointInfo.RequestParameters = new List<SwaggerRequestParameter>();
            }

            endpointInfo.RequestParameters.Add(new SwaggerRequestParameter
            {
                Required = required,
                Description = description,
                In = loc,
                Name = name,
                Schema = new SchemaRef
                {
                    Ref = "#/definitions/" + GetOrSaveSchemaReference(requestType)
                }
            });

            return endpointInfo;
        }

        public static SwaggerEndpointInfo WithDescription(this SwaggerEndpointInfo endpointInfo, string description, string[] contentType = null, params string[] tags)
        {
            if (endpointInfo.Tags == null)
            {
                if (tags.Length == 0)
                {
                    tags = new[] { "default" };
                }

                endpointInfo.Tags = tags;
            }

            if (endpointInfo.ContentType == null)
            {
                if (contentType == null)
                {
                    contentType = new[] { "application/json" };
                }

                endpointInfo.ContentType = contentType;
            }

            endpointInfo.Description = description;

            return endpointInfo;
        }

        public static SwaggerEndpointInfo WithSummary(this SwaggerEndpointInfo endpointInfo, string summary)
        {
            endpointInfo.Summary = summary;
            return endpointInfo;
        }

        public static SwaggerEndpointInfo WithTags(this SwaggerEndpointInfo endpointInfo, IEnumerable<string> tags)
        {
            endpointInfo.Tags = tags.ToArray();
            return endpointInfo;
        }

        private static SwaggerResponseInfo GenerateResponseInfo(string description, Type responseType)
           => new SwaggerResponseInfo
           {
               Schema = new SchemaRef
               {
                   Ref = "#/definitions/" + GetOrSaveSchemaReference(responseType)
               },
               Description = description
           };

        private static SwaggerResponseInfo GenerateResponseInfo(string description)
            => new SwaggerResponseInfo
            {
                Description = description
            };

        private static string GetOrSaveSchemaReference(Type type)
        {
            string key = type.FullName;

            if (SchemaCache.Cache.ContainsKey(key))
            {
                return key;
            }

            var schema = JsonSchema.FromType(type, new NJsonSchema.Generation.JsonSchemaGeneratorSettings
            {
                SchemaType = SchemaType.Swagger2,
                TypeNameGenerator = new TypeNameGenerator()
            });

            SchemaCache.Cache[key] = schema;

            return key;
        }
    }
}
