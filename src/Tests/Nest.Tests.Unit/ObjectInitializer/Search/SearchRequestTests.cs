﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Elasticsearch.Net;
using FluentAssertions;
using Nest.Resolvers;
using Nest.Tests.MockData.Domain;
using NUnit.Framework;

namespace Nest.Tests.Unit.ObjectInitializer.Search
{
	[TestFixture]
	public class SearchRequestTests : BaseJsonTests
	{
		private readonly IElasticsearchResponse _status;

		public SearchRequestTests()
		{
			QueryContainer query = new TermQuery()
			{
				Field = Property.Path<ElasticsearchProject>(p=>p.Name),
				Value = "value"
			} && new PrefixQuery()
			{
				Field = "prefix_field", 
				Value = "prefi", 
				Rewrite = RewriteMultiTerm.ConstantScoreBoolean
			};

			var request = new SearchRequest<ElasticsearchProject>
			{
				From = 0,
				Size = 20,
				Explain = true,
				TrackScores = true,
				MinScore = 2.1,
				IndicesBoost = new Dictionary<IndexNameMarker, double>
				{
					{ Infer.Index<ElasticsearchProject>(), 2.3 }
				},
				Sort = new List<ISort>()
				{
					new Sort { Field = "field", Order = SortOrder.Ascending, Missing = "_first"}
				},
				Suggest = new Dictionary<string, ISuggestBucket>
				{
					{
						"suggestion", new SuggestBucket
						{
							Text = "suggest me",
							Completion = new CompletionSuggester
							{
								Analyzer = "standard",
								Field = Property.Path<ElasticsearchProject>(p=>p.Content),
								Size = 4,
								ShardSize = 10,
								Fuzzy = new FuzzySuggester
								{
									Fuzziness = Fuzziness.Ratio(0.3),
									PrefixLength = 4
								}

							}
						}
					}
				},
				Rescore = new Rescore
				{
					WindowSize = 10,
					Query = new RescoreQuery
					{
						Query = new TermQuery() { }.ToContainer(),
						QueryWeight = 1.2,
						RescoreQueryWeight = 2.1
					}
				},
				Fields = new[]
				{
					"field",
					Property.Path<ElasticsearchProject>(p=>p.Name)
				},
				ScriptFields = new FluentDictionary<string, IScriptQuery>()
					.Add("script_field_name", new ScriptQuery
					{
						Script = "doc['loc'].value * multiplier",
						Params = new Dictionary<string, object>
						{
							{"multiplier", 4}
						}
					}),
				Source = new SourceFilter
				{
					Include = new PropertyPathMarker[]
					{
						"na*"
					}
				},
				Aggregations = new Dictionary<string, IAggregationContainer>
				{
					{ "my_agg", new AggregationContainer
					{
						Terms = new TermsAggregator
						{
							Field = Property.Path<ElasticsearchProject>(p=>p.Name),
							Size = 10,
							ExecutionHint = TermsAggregationExecutionHint.GlobalOrdinals,
						},
						Aggregations = new Dictionary<string, IAggregationContainer>
						{
							{ "max_count", new AggregationContainer()
							{
								Max = new MaxAggregator()
								{
									Field = "loc"
								}
							}
							}
						}
					}}
				},
				Query = query,
				PostFilter = new QueryContainer(new BoolQuery
				{
					//TODO Cache = true,
					Must = new QueryContainer[]
					{
						new TermQuery { Field = "value", Value = "asdasd"}
					}
				})
			};
			var response = this._client.Search<ElasticsearchProject>(request);
			this._status = response.ConnectionStatus;
		}

		[Test]
		public void Url()
		{
			this._status.RequestUrl.Should().EndWith("/nest_test_data/elasticsearchprojects/_search");
			this._status.RequestMethod.Should().Be("POST");
		}
		
		[Test]
		public void SearchBody()
		{
			this.JsonEquals(this._status.Request, MethodBase.GetCurrentMethod());
		}
	}
}
