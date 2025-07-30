namespace Pulse360.Models
{
    
		public class KnowledgeBaseTopic
		{
			public int Id { get; set; }
			public string MasterTopic { get; set; }

			// Navigation property for SubTopics
			public List<SubTopic> SubTopics { get; set; } = new List<SubTopic>();
		}

		public class SubTopic
		{
			public int Id { get; set; }
			public string Title { get; set; }

			// Foreign key to KnowledgeBaseTopic
			public int KnowledgeBaseTopicId { get; set; }
			public KnowledgeBaseTopic KnowledgeBaseTopic { get; set; }
		}
	}

