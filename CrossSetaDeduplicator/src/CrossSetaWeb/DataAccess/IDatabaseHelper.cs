using System.Collections.Generic;
using CrossSetaWeb.Models;

namespace CrossSetaWeb.DataAccess
{
    public interface IDatabaseHelper
    {
        void InsertLearner(LearnerModel learner);
        void InitializeHomeAffairsTable();
        void BatchImportHomeAffairsData(List<HomeAffairsCitizen> citizens);
        List<DatabaseHelper.BulkInsertError> BatchInsertLearners(List<LearnerModel> learners);
        void InsertUser(UserModel user);
        List<LearnerModel> GetAllLearners();
        LearnerModel? GetLearnerByNationalID(string nationalID);
        HomeAffairsCitizen GetHomeAffairsCitizen(string nationalID);
    }
}
