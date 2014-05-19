﻿using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Nest.Resolvers.Converters.Queries
{
	public class TermsQueryJsonConverter : JsonConverter
	{
		public override bool CanRead { get { return true; } }
		public override bool CanWrite { get { return true; } }

		public override bool CanConvert(Type objectType)
		{
			return true; //only to be used with attribute or contract registration.
		}
		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			var t = value as ITermsQuery;
			if (t == null) return;

			string field = null;

			var contract = serializer.ContractResolver as ElasticContractResolver;
			if (contract != null && contract.ConnectionSettings != null)
				field = contract.Infer.PropertyPath(t.Field);

			writer.WriteStartObject();
			{
				if (t.Terms.HasAny())
				{
					writer.WritePropertyName(field);
					serializer.Serialize(writer, t.Terms);
					//writer.WriteStartArray();
					//foreach(var term in t.Terms)
					//	writer.WriteValue(term);
					//writer.WriteEndArray();
				}
				else if (t.ExternalField != null)
				{
					writer.WritePropertyName(field);
					serializer.Serialize(writer, t.ExternalField);
				}
				if (t.DisableCoord.HasValue)
				{
					writer.WritePropertyName("disable_coord");
					writer.WriteValue(t.DisableCoord.Value);
				}
				if (t.MinimumShouldMatch.HasValue)
				{
					writer.WritePropertyName("minimum_should_match");
					writer.WriteValue(t.MinimumShouldMatch.Value);
				}
				if (t.Boost.HasValue)
				{
					writer.WritePropertyName("boost");
					writer.WriteValue(t.Boost.Value);
				}
				
			}
			writer.WriteEndObject();
		}
		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			var filter = new TermsQueryDescriptor<object, object>();
			ITermsQuery f = filter;
			if (reader.TokenType != JsonToken.StartObject)
				return null;

			var depth = reader.Depth;
			while (reader.Read() && reader.Depth >= depth && reader.Value != null)
			{
				var property = reader.Value as string;
				switch (property)
				{
					case "disable_coord":
						reader.Read();
						f.DisableCoord = reader.Value as bool?;
						break;
					case "minimum_should_match":
						reader.Read();
						f.MinimumShouldMatch = reader.Value as int?;
						break;
					case "boost":
						reader.Read();
						f.Boost = reader.Value as double?;
						break;
					default:
						f.Field = property;
						//reader.Read();
						ReadTerms(f, reader);
						//reader.Read();
						break;
				}
			}
			return filter;

		}

		private void ReadTerms(ITermsQuery termsQuery, JsonReader reader)
		{
			reader.Read();
			if (reader.TokenType == JsonToken.StartObject)
			{
				var ef = new ExternalFieldDeclarationDescriptor<object>();
				var depth = reader.Depth;
				while (reader.Read() && reader.Depth >= depth && reader.Value != null)
				{
					var property = reader.Value as string;
					switch (property)
					{
						case "id":
							reader.Read();
							ef._Id = reader.Value as string;
							break;
						case "index":
							reader.Read();
							ef._Index = reader.Value as string;
							break;
						case "type":
							reader.Read();
							ef._Type = reader.Value as string;
							break;
						case "path":
							reader.Read();
							ef._Path = reader.Value as string;
							break;
					}
				}
				termsQuery.ExternalField = ef;
			}
			else if (reader.TokenType == JsonToken.StartArray)
			{
				var values = JArray.Load(reader).Values<string>();
				termsQuery.Terms = values;
			}
		}
	}

}