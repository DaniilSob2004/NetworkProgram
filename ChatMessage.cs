using System;

namespace NetworkProgram
{
    public class ChatMessage
    {
        public string Login { get; set; } = null!;
        public string Text { get; set; } = null!;
        public DateTime Moment { get; set; }  // дата отправки

        public override string ToString()
        {
            return $"{Login}: {Text}";
        }

        public string GetSendTime()
        {
            DateTime now = DateTime.Now;
            TimeSpan date = now - Moment;
            string sendTime = "";
            if (now.Date > Moment.Date)  // если прошло больше суток
            {
                sendTime = date.ToString("dd") + " д. назад";
            }
            else if (now.Date == Moment.Date)  // если это сегодняшний день
            {
                if (date.TotalSeconds >= 60)
                {
                    sendTime = date.ToString("mm") + " мин. назад";
                }
                else if (date.TotalMinutes >= 60)
                {
                    sendTime = date.ToString("HH") + " ч. назад";
                }
                else
                {
                    sendTime = date.ToString("ss") + " сек. назад";
                }
            }
            return sendTime;
        }
    }
}
