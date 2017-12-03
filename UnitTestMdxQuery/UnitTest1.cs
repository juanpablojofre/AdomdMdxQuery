using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.AnalysisServices.AdomdClient;

namespace AdomdMdx2017
{
  [TestClass]
  public class UnitTest1
  {
    [TestMethod]
    public void TestConstructor()
    {
      string instance = @"JUAMPA-DEVPC\Cubes";
      string database = "AdventureWorksDW2014Multidimensional-EE";
      string query = @"select 
	NON EMPTY 
		{
			[Measures].[Order Quantity],
			[Measures].[Sales Amount]
		} *
		{
			DESCENDANTS([Date].[Calendar].[Calendar Year].&[2013], 2, self), 
			[Date].[Calendar].&[2013]
		} ON 0,
	NON EMPTY CROSSJOIN( 
		{
			DESCENDANTS ([Promotion].[Promotions].[Category],,SELF)
		}, 
		{
			DESCENDANTS ([Product].[Product Categories].[Subcategory].[Gloves], ,SELF_AND_AFTER),
			DESCENDANTS ([Product].[Product Categories].[Subcategory].[Jerseys], ,SELF_AND_AFTER)
		}) on 1
from [Adventure Works]
where ([Sales Territory].[Sales Territory].[Group].&[North America])";
      string expected = @"Axis1	Order Quantity | Q1 CY 2013	Order Quantity | Q2 CY 2013	Order Quantity | Q3 CY 2013	Order Quantity | Q4 CY 2013	Order Quantity | CY 2013	Sales Amount | Q1 CY 2013	Sales Amount | Q2 CY 2013	Sales Amount | Q3 CY 2013	Sales Amount | Q4 CY 2013	Sales Amount | CY 2013
No Discount | Gloves	833	647	464	538	2482	13376.438	11182.134	8571.5	10089.88	43219.9519999999
No Discount | Half-Finger Gloves, L	167	103	79	128	477	2757.574	1875.934	1640.83	2478.388	8752.72599999999
No Discount | Half-Finger Gloves, M	376	308	203	254	1141	5936.37599999999	5231.06399999999	3649.01	4515.95599999999	19332.406
No Discount | Half-Finger Gloves, S	290	236	182	156	864	4682.48799999999	4075.13599999999	3281.66	3095.53599999999	15134.82
No Discount | Jerseys	2093	1950	1625	1549	7217	70495.658	68552.4439999999	59000.262	58338.7979999999	256387.162
No Discount | Long-Sleeve Logo Jersey, L	396	434	306	271	1407	12617.476	14237.152	10557.888	9548.08999999999	46960.606
No Discount | Long-Sleeve Logo Jersey, M	313	247	221	234	1015	10107.978	8308.33799999999	7668.466	8638.27199999998	34723.054
No Discount | Long-Sleeve Logo Jersey, S	28	56	74	78	236	1399.72	2799.44	3699.26	3899.22	11797.64
No Discount | Long-Sleeve Logo Jersey, XL	265	155	164	119	703	8588.28199999999	5608.87799999999	5738.852	4669.06599999999	24605.078
No Discount | Short-Sleeve Classic Jersey, L	374	359	291	264	1288	12806.428	12234.134	10398.474	9588.62399999999	45027.66
No Discount | Short-Sleeve Classic Jersey, M	28	40	40	40	148	1511.72	2159.6	2159.6	2159.6	7990.52
No Discount | Short-Sleeve Classic Jersey, S	298	275	202	205	980	10258.1	10052.938	7407.428	7677.37799999999	35395.844
No Discount | Short-Sleeve Classic Jersey, XL	391	384	327	338	1440	13205.954	13151.964	11370.294	12158.548	49886.76
Reseller | Gloves	252	55	40	41	388	3557.3635	896.7234	747.0626	760.9827	5962.1322
Reseller | Half-Finger Gloves, L	21	14	18	18	71	398.0213	342.86	440.82	440.82	1622.5213
Reseller | Half-Finger Gloves, M	168	41	11	23	243	2282.3749	553.8634	153.1213	320.1627	3309.5223
Reseller | Half-Finger Gloves, S	63		11		74	876.9673		153.1213		1030.0886
Reseller | Jerseys	1135	359	255	302	2051	32778.9676	10493.7515	7851.9241	9034.9179	60159.5611
Reseller | Long-Sleeve Logo Jersey, L	268	81	79	88	516	7169.8958	2264.8469	2208.0183	2461.4527	14104.2137
Reseller | Long-Sleeve Logo Jersey, M	119	23			142	3305.5838	653.5293			3959.1131
Reseller | Long-Sleeve Logo Jersey, XL	46	9	19	7	81	1565.9668	449.91	949.81	349.93	3315.6168
Reseller | Short-Sleeve Classic Jersey, L	200	61	36	91	388	5892.2472	1755.4902	1104.765	2792.6003	11545.1027
Reseller | Short-Sleeve Classic Jersey, S	103	11			114	3160.8553	337.5671			3498.4224
Reseller | Short-Sleeve Classic Jersey, XL	399	174	121	116	810	11684.4187	5032.408	3589.3308	3430.9349	23737.0924
";
      MdxQuery q1 = new MdxQuery(instance, database, query);
      string actual = q1["default"];
      Assert.AreEqual(expected, actual, "Returned table different than 'expected'");
    }

    [TestMethod]
    public void TestFlattenFourDimensions()
    {
      string instance = @"JUAMPA-DEVPC\Cubes";
      string database = "AdventureWorksDW2014Multidimensional-EE";
      string query = @"SELECT 
	{[Measures].[Order Quantity], [Measures].[Sales Amount]} ON 0,
	NON EMPTY {Descendants([Date].[Calendar].[All Periods],1,SELF), Descendants([Date].[Calendar].[All Periods],3,SELF)}  ON 1,
	NON EMPTY {DESCENDANTS([Product].[Product Categories].[All Products],1,SELF), DESCENDANTS([Product].[Product Categories].[All Products],2,SELF)} ON 2,
	NON EMPTY {DESCENDANTS([Sales Territory].[Sales Territory].[All Sales Territories],1,SELF), DESCENDANTS([Sales Territory].[Sales Territory].[All Sales Territories],2,SELF)} ON 3
FROM [Adventure Works]";
      string expected = @"Axis1	Order Quantity | Q1 CY 2013	Order Quantity | Q2 CY 2013	Order Quantity | Q3 CY 2013	Order Quantity | Q4 CY 2013	Order Quantity | CY 2013	Sales Amount | Q1 CY 2013	Sales Amount | Q2 CY 2013	Sales Amount | Q3 CY 2013	Sales Amount | Q4 CY 2013	Sales Amount | CY 2013
No Discount | Gloves	833	647	464	538	2482	13376.438	11182.134	8571.5	10089.88	43219.9519999999
No Discount | Half-Finger Gloves, L	167	103	79	128	477	2757.574	1875.934	1640.83	2478.388	8752.72599999999
No Discount | Half-Finger Gloves, M	376	308	203	254	1141	5936.37599999999	5231.06399999999	3649.01	4515.95599999999	19332.406
No Discount | Half-Finger Gloves, S	290	236	182	156	864	4682.48799999999	4075.13599999999	3281.66	3095.53599999999	15134.82
No Discount | Jerseys	2093	1950	1625	1549	7217	70495.658	68552.4439999999	59000.262	58338.7979999999	256387.162
No Discount | Long-Sleeve Logo Jersey, L	396	434	306	271	1407	12617.476	14237.152	10557.888	9548.08999999999	46960.606
No Discount | Long-Sleeve Logo Jersey, M	313	247	221	234	1015	10107.978	8308.33799999999	7668.466	8638.27199999998	34723.054
No Discount | Long-Sleeve Logo Jersey, S	28	56	74	78	236	1399.72	2799.44	3699.26	3899.22	11797.64
No Discount | Long-Sleeve Logo Jersey, XL	265	155	164	119	703	8588.28199999999	5608.87799999999	5738.852	4669.06599999999	24605.078
No Discount | Short-Sleeve Classic Jersey, L	374	359	291	264	1288	12806.428	12234.134	10398.474	9588.62399999999	45027.66
No Discount | Short-Sleeve Classic Jersey, M	28	40	40	40	148	1511.72	2159.6	2159.6	2159.6	7990.52
No Discount | Short-Sleeve Classic Jersey, S	298	275	202	205	980	10258.1	10052.938	7407.428	7677.37799999999	35395.844
No Discount | Short-Sleeve Classic Jersey, XL	391	384	327	338	1440	13205.954	13151.964	11370.294	12158.548	49886.76
Reseller | Gloves	252	55	40	41	388	3557.3635	896.7234	747.0626	760.9827	5962.1322
Reseller | Half-Finger Gloves, L	21	14	18	18	71	398.0213	342.86	440.82	440.82	1622.5213
Reseller | Half-Finger Gloves, M	168	41	11	23	243	2282.3749	553.8634	153.1213	320.1627	3309.5223
Reseller | Half-Finger Gloves, S	63		11		74	876.9673		153.1213		1030.0886
Reseller | Jerseys	1135	359	255	302	2051	32778.9676	10493.7515	7851.9241	9034.9179	60159.5611
Reseller | Long-Sleeve Logo Jersey, L	268	81	79	88	516	7169.8958	2264.8469	2208.0183	2461.4527	14104.2137
Reseller | Long-Sleeve Logo Jersey, M	119	23			142	3305.5838	653.5293			3959.1131
Reseller | Long-Sleeve Logo Jersey, XL	46	9	19	7	81	1565.9668	449.91	949.81	349.93	3315.6168
Reseller | Short-Sleeve Classic Jersey, L	200	61	36	91	388	5892.2472	1755.4902	1104.765	2792.6003	11545.1027
Reseller | Short-Sleeve Classic Jersey, S	103	11			114	3160.8553	337.5671			3498.4224
Reseller | Short-Sleeve Classic Jersey, XL	399	174	121	116	810	11684.4187	5032.408	3589.3308	3430.9349	23737.0924
";
      MdxQuery q1 = new MdxQuery(instance, database, query);
      string actual = q1["default"];
      Assert.AreEqual(expected, actual, "Returned table different than 'expected'");
    }
  }
}
