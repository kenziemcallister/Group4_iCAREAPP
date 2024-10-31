//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Group4_iCAREAPP.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class PatientRecord
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public PatientRecord()
        {
            this.TreatmentRecord = new HashSet<TreatmentRecord>();
        }
    
        public string ID { get; set; }
        public string name { get; set; }
        public string address { get; set; }
        public Nullable<System.DateTime> dateOfBirth { get; set; }
        public Nullable<double> weight { get; set; }
        public Nullable<double> height { get; set; }
        public string bloodGroup { get; set; }
        public string bedID { get; set; }
        public string treatmentArea { get; set; }
        public string geographicalUnit { get; set; }
        public string docID { get; set; }
        public string modifierID { get; set; }
        public Nullable<int> doctorCount { get; set; }
        public Nullable<int> nurseCount { get; set; }
    
        public virtual DocumentMetadata DocumentMetadata { get; set; }
        public virtual GeoCodes GeoCodes { get; set; }
        public virtual ModificationHistory ModificationHistory { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TreatmentRecord> TreatmentRecord { get; set; }
    }
}
