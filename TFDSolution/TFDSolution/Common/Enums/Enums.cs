using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace TFDSolution.Common.Enums
{
	public class Enums
	{
		public enum EventPriority
        {
            Low = 1,
            Medium = 2,
            High = 3
            // Add more priorities as needed
        }
        public enum FollowupStatus
        {
            Open = 1,
            Lost = 2,
            Win = 3,
            Hot = 4,
            // Add more Status as needed
            // Close = 9

        }
        public enum CommunicationModes
        {
            [Display(Name = "Personally Meet")]
            PersonallyMeet = 1,

            [Display(Name = "Phone")]
            Phone = 2,

            [Display(Name = "By Mail")]
            ByMail = 3
            // Add more priorities as needed
        }
    }
}