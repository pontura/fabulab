namespace Yaguar.StoryMaker.Editor
{
    public class ObjectSignal : Objeto
    {
        public TMPro.TMP_Text field;
        public virtual void SetField(string text)
        {
            field.text = text;
            GetData().customization = text;
        }
        public virtual void SetDirection(int direction)
        {
        }

        public virtual void SetFont(int fontId) {
        }

        public override void OnInit()
        {
            SOData soData = GetData();
            if (soData == null)
                return;
            if (soData.customization != null && soData.customization.Length > 0)
                SetField(soData.customization);

        }
    }
}
