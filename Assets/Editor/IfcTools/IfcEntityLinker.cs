using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using Xbim.Ifc;
using Xbim.Ifc4.Interfaces;

/// <summary>
/// Links elements from the scene to related IFC entities
/// </summary>
public class IfcEntityLinker
{


    #region public methods
    /// <summary>
    /// Links the entities of the IFC model to the game objects under the given root object by using the names of the child objects.
    /// </summary>
    /// <param name="ifcGameObject"></param>
    /// <param name="ifcModel"></param>
    public void LinkIfcEntitiesByName(GameObject ifcGameObject, IfcStore ifcModel)
    {
        this.AddIfcModelData(ifcGameObject, ifcModel);

        foreach (Transform child in ifcGameObject.transform)
        {
            //retrieve the IFC product
            IIfcProduct ifcProduct = ifcModel.Model.Instances.OfType<IIfcProduct>().FirstOrDefault(x => x.GlobalId == child.gameObject.name);

            IfcProductData ifcProductData = this.GetComponentOrNew<IfcProductData>(child.gameObject);
            ifcProductData.Product = ifcProduct;

            if (ifcProduct.Name.HasValue)
            {
                child.gameObject.name = ifcProduct.Name.Value;
            }
            else
            {
                Debug.LogWarning("product: " + ifcProduct.GlobalId.ToString() + " does not have a name");
            }

            this.AddPropertiesOfProduct(child.gameObject, ifcProduct);
        }
    }

    /// <summary>
    /// Links all game objects to their related ifc entity by using the attached IfcProductData component
    /// </summary>
    /// <param name="ifcGameObject"></param>
    /// <param name="ifcModel"></param>
    public void LinkEntitiesByProductData(GameObject ifcGameObject, IfcStore ifcModel)
    {
        this.AddIfcModelData(ifcGameObject, ifcModel);
        this.RecursiveEntityLinking(ifcGameObject, ifcModel);
    }

    /// <summary>
    /// generates and applies the hierarchy for the game object considering the attached IFC model
    /// </summary>
    /// <param name="ifcGameObject"></param>
    public void GenerateIfcElementHierarchy (GameObject ifcGameObject)
    {
        IfcFileAssociation fileAssoc = ifcGameObject.GetComponent<IfcFileAssociation>();
        if (fileAssoc == null)
        {
            Debug.LogError("Could not find an associated IFC model in " + ifcGameObject.name);
            return;
        }

        //collect IFC related Objects
        Dictionary<int,GameObject> productGameObjects = new Dictionary<int, GameObject>();
        foreach (Transform child in ifcGameObject.transform)
        {
            IfcProductData productData = child.gameObject.GetComponent<IfcProductData>();
            if (productData != null)
            {
                productGameObjects.Add(productData.Label, child.gameObject);
            }
        }

        //Hierarchy: Project --> Site --> Building --> Floor --> Expresstype --> element type --> element
        IfcProjectData projectData = ifcGameObject.GetComponent<IfcProjectData>();
        foreach (IIfcRelAggregates projectDecomp in projectData.IfcProject.IsDecomposedBy)
        {
            //maybe several related objects
            foreach (IIfcObjectDefinition projectPart in projectDecomp.RelatedObjects)
            {
                if (projectPart is IIfcSite site)
                    this.RecursiveProductDecomposition(site, ifcGameObject, new List<GameObject>(), productGameObjects);
            }
        }
    }

    private GameObject GenerateGameObjectForProductType(IIfcTypeObject typeObject, GameObject gameObjectExpressType)
    {
        
        Transform productTypeTransform =
            gameObjectExpressType.transform.Find(typeObject.Name.Value);
        if (productTypeTransform != null)
        {
            return productTypeTransform.gameObject;
        }
        else
        {
            GameObject productTypeObject =
                new GameObject(typeObject.Name.Value);
            IfcTypeData data = productTypeObject.AddComponent<IfcTypeData>();
            data.TypeObject = typeObject;
            productTypeObject.transform
                .SetParent(gameObjectExpressType.transform);
            return productTypeObject;
        }

    }

    /// <summary>
    /// Links the materials to the game objects
    /// </summary>
    /// <param name="ifcRootObject"></param>
    /// <param name="unityMaterials"></param>
    public void LinkMaterials(GameObject ifcRootObject, IEnumerable<IfcUnityMaterialLink> unityMaterials)
    {
        //generate the material mappings
        GameObject materialsObject = new GameObject("Materials");
        materialsObject.transform.SetParent(ifcRootObject.transform);
        materialsObject.transform.SetAsFirstSibling();
        MaterialMap materialMap = materialsObject.AddComponent<MaterialMap>();
        materialMap.BuildingModel = ifcRootObject;

        //link the appropriate materials
        IfcStore ifcModel = ifcRootObject.GetComponent<IfcFileAssociation>().IfcModel;
        foreach (IIfcMaterial ifcMaterial in ifcModel.Instances.OfType<IIfcMaterial>())
        {
            MaterialMapKeyValue entry = new MaterialMapKeyValue();
            entry.IfcMaterial = ifcMaterial;
            materialMap.Add(entry.IfcLabel, entry);

            //add existing corresponding material
            if (unityMaterials != null)
            {
                //string comparingName = entry.IfcMaterialName.ToLower().Replace('/', '-').Replace(' ', '-');
                //Material correspondingMaterial = unityMaterials.FirstOrDefault(x => x.name.Contains(comparingName));
                IfcUnityMaterialLink materialLink = unityMaterials.FirstOrDefault(x => x.IfcLabel == entry.SurfaceStyleLabel);
                if (materialLink != null)
                {
                    entry.CorrespondingMaterial = materialLink.UnityMaterial;
                }
            }
        }

#if (UNITY_EDITOR)
        // if the linking is done in the editor, mark the entity
        UnityEditor.Selection.objects = new UnityEngine.Object[] { materialsObject };
#endif

        //apply materials
        if (unityMaterials != null)
        {
            materialMap.ApplyMaterials();
        }
    }

    #endregion public methods

    //==============================================================================
    //==============================================================================

    #region helper methods

    /// <summary>
    /// adds the project data and the file association to the model object in unity
    /// </summary>
    /// <param name="ifcGameObject"></param>
    /// <param name="ifcModel"></param>
    private void AddIfcModelData(GameObject ifcGameObject, IfcStore ifcModel)
    {
        IfcFileAssociation fileAssoc = this.GetComponentOrNew<IfcFileAssociation>(ifcGameObject);
        fileAssoc.IfcModel = ifcModel;

        IfcProjectData projectData = GetComponentOrNew<IfcProjectData>(ifcGameObject);
        IIfcProject ifcProject = ifcModel.Model.Instances.OfType<IIfcProject>().FirstOrDefault();
        projectData.IfcProject = ifcProject;
    }

    /// <summary>
    /// Get the IfcFileAssociation from the given game object
    /// </summary>
    /// <param name="gameObject"></param>
    /// <returns>the available IfcFileAssociation if attached to the game object; else attaches a new one to the game object and return this.</returns>
    private T GetComponentOrNew<T>(GameObject gameObject) where T : UnityEngine.Component
    {
        T fileAssoc = gameObject.GetComponent<T>();
        if (fileAssoc == null)
        {
            fileAssoc = gameObject.AddComponent<T>();
        }
        return fileAssoc;
    }

    /// <summary>
    /// recursive linking of entitites after loading a saved project. 
    /// </summary>
    /// <param name="ifcGameObject"></param>
    /// <param name="ifcModel"></param>
    private void RecursiveEntityLinking(GameObject ifcGameObject, IfcStore ifcModel)
    {
        //a new instance should not be created, because we assume that this method is run only after loading a unity project
        IfcProductData ifcProductData = ifcGameObject.GetComponent<IfcProductData>();
        if (ifcProductData != null)
        {
            //retrieve the ifc product
            IIfcProduct ifcProduct = ifcModel.Model.Instances.OfType<IIfcProduct>().FirstOrDefault(x => x.GlobalId == ifcProductData.Id);
            if (ifcProduct != null)
            {
                ifcProductData.Product = ifcProduct;
                this.AddPropertiesOfProduct(ifcGameObject, ifcProduct);
            }
        }
        
        //recursive scanning
        foreach (Transform child in ifcGameObject.transform)
        {   
           this.RecursiveEntityLinking(child.gameObject, ifcModel); 
        }
    }

    /// <summary>
    /// Generates a game object based on an ifcProduct
    /// </summary>
    /// <param name="product"></param>
    /// <returns></returns>
    private GameObject GenerateGameObjectByIfcProduct(IIfcProduct product)
    {
        GameObject gameObjectProduct = new GameObject();
        gameObjectProduct.name = product.ExpressType + ": " + product.Name.Value;
        IfcProductData productSite = gameObjectProduct.AddComponent<IfcProductData>();
        productSite.Product = product;
        return gameObjectProduct;
    }

    /// <summary>
    /// Recursive method to decompose product hierarchy
    /// </summary>
    /// <param name="ifcProduct"></param>
    /// <param name="parentObject"></param>
    /// <param name="expressTypes"></param>
    /// <param name="productGameObjects"></param>
    /// <returns></returns>
    private GameObject RecursiveProductDecomposition(IIfcProduct ifcProduct, GameObject parentObject, List<GameObject> expressTypes, Dictionary<int, GameObject> productGameObjects)
    {
        //an object is valid if it has subobjects or if it is already existing in the scene
        bool validObject = false;
        GameObject result;
        if (productGameObjects.ContainsKey(ifcProduct.EntityLabel))
        {
            result = productGameObjects[ifcProduct.EntityLabel];
            validObject = true;
        }
        else
        {
            result = this.GenerateGameObjectByIfcProduct(ifcProduct);
        }

        //use new list of express types if a storey is reached
        List<GameObject> localExpressTypes;

        //decomposition of spatial elements (storey)
        if (ifcProduct is IIfcSpatialElement spatialElement)
        {
            localExpressTypes = new List<GameObject>();
            foreach (IIfcRelContainedInSpatialStructure decompositionRelation in spatialElement.ContainsElements)
            {
                foreach (IIfcProduct containedElement in decompositionRelation.RelatedElements)
                {
                    validObject = true;
                    RecursiveProductDecomposition(containedElement, result, localExpressTypes, productGameObjects);
                }
            }
        }
        else
        {
            localExpressTypes = expressTypes;
        }

        // generate type objects 
        IIfcRelDefinesByType typeObjectRelation = ifcProduct.IsTypedBy.FirstOrDefault();
        if (typeObjectRelation != null
            && typeObjectRelation.RelatingType != null)
        {
            GameObject gameObjectExpressType = localExpressTypes.FirstOrDefault(x => x.name == ifcProduct.ExpressType.ExpressName);
            if (gameObjectExpressType == null)
            {
                gameObjectExpressType = new GameObject();
                gameObjectExpressType.name = ifcProduct.ExpressType.ExpressName;
                gameObjectExpressType.transform.SetParent(parentObject.transform);
                localExpressTypes.Add(gameObjectExpressType);
            }
            GameObject typeGameObject = this.GenerateGameObjectForProductType(typeObjectRelation.RelatingType, gameObjectExpressType);
            result.transform.SetParent(typeGameObject.transform);
        }
        else
        {
            result.transform.SetParent(parentObject.transform);
        }

        // generate decompositions

        List<GameObject> subExpressObjects = new List<GameObject>();
        foreach (IIfcRelAggregates decompositionRelation in ifcProduct.IsDecomposedBy)
        {
            foreach (IIfcObjectDefinition partObject in decompositionRelation.RelatedObjects)
            {
                if (partObject is IIfcProduct partProduct)
                {
                    RecursiveProductDecomposition(partProduct, result, subExpressObjects, productGameObjects);
                    validObject = true;
                }
            }
        }

        if (!validObject)
            UnityEngine.Object.DestroyImmediate(result);

        return result;
    }

    /// <summary>
    /// Adds the properties of a product to the game object
    /// </summary>
    /// <param name="ifcGameObject">Game object to attach properties</param>
    /// <param name="ifcProduct">IFC product to be searched for properties</param>
    private void AddPropertiesOfProduct(GameObject ifcGameObject, IIfcProduct ifcProduct)
    {
        foreach (IIfcRelDefinesByProperties propertyRelation in ifcProduct.IsDefinedBy)
        {
            if (propertyRelation.RelatingPropertyDefinition is IIfcPropertySet propertySet)
            {
                IfcPropertySetData propertyData =
                    ifcGameObject.GetComponents<IfcPropertySetData>().FirstOrDefault(x => x.Label == propertySet.EntityLabel);
                if (propertyData == null)
                    propertyData = ifcGameObject.AddComponent<IfcPropertySetData>();
                propertyData.PropertySet = propertySet;
            }
        }
    }
    #endregion helper methods
}