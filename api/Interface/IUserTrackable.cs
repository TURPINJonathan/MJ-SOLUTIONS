namespace api.Interface
{
	public interface IUserTrackable
	{
		int CreatedById { get; set; }
		int? UpdatedById { get; set; }
		int? DeletedById { get; set; }
	}

	public interface IPublishableUserTrackable : IUserTrackable
	{
		int? PublishedById { get; set; }
	}

}