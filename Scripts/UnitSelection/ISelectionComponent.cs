namespace Nashet.UnitSelection
{
	public interface ISelectionComponent
	{
		event EntityClickedDelegate OnEntitySelected;
		event EntityClickedDelegate OnMultipleEntitiesSelected;
	}
}