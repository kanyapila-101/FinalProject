
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------


namespace Students_Attendance_Project.Models
{

using System;
    using System.Collections.Generic;
    
public partial class Tb_Group
{

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
    public Tb_Group()
    {

        this.Tb_GroupTask = new HashSet<Tb_GroupTask>();

    }


    public int GroupID { get; set; }

    public string GroupName { get; set; }

    public Nullable<int> TaskID { get; set; }

    public Nullable<int> StudyGroupID { get; set; }



    public virtual Tb_StudyGroup Tb_StudyGroup { get; set; }

    public virtual Tb_Task Tb_Task { get; set; }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]

    public virtual ICollection<Tb_GroupTask> Tb_GroupTask { get; set; }

}

}