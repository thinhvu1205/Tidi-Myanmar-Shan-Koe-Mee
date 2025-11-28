using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Extension;

public class IAPManager : IDetailedStoreListener
{
    public static IAPManager Instance { get; private set; }

    private IStoreController controller;
    private IExtensionProvider extensions;
    private JArray lsItem; // lưu danh sách sản phẩm

    public IAPManager(JObject jData)
    {
        if (Instance != null)
        {
            Debug.LogWarning("IAPManager already initialized!");
            return;
        }

        Instance = this;
        lsItem = (JArray)jData["items"];
        InitIAP();
    }
    public void InitIAP()
    {
        if (IsInitialized())
        {
            Debug.Log("IAP already initialized.");
            return;
        }

        var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());
        for (int i = 0; i < lsItem.Count; i++)
        {
            var itData = (JObject)lsItem[i];
            builder.AddProduct((string)itData["url"], ProductType.Consumable);
            Debug.Log("IAPManager: added product " + (string)itData["url"]);
        }

        Debug.Log("IAP initialization requested.");
        UnityPurchasing.Initialize(this, builder);
    }

    public bool IsInitialized()
    {
        return controller != null && extensions != null;
    }

    // -------------------- Giữ nguyên buyIAP --------------------
    public void buyIAP(string productId)
    {
        Debug.Log("buyIAP  " + productId);
        if (!IsInitialized())
        {
            Debug.LogWarning("IAP not initialized yet. Cannot buy: " + productId);
            return;
        }

        Product product = controller.products.WithID(productId);
        if (product == null)
        {
            Debug.LogError("IAP product not found: " + productId + ". Check SKU/config in store and JSON config.");
            return;
        }

        if (!product.availableToPurchase)
        {
            Debug.LogWarning("IAP product not availableToPurchase: " + productId);
            return;
        }

        controller.InitiatePurchase(product);
    }

    void IDetailedStoreListener.OnPurchaseFailed(Product product, PurchaseFailureDescription failureDescription)
    {
        Debug.Log($"Purchase failed (deprecated interface): {failureDescription.reason} - {failureDescription.message}");
    }
    void IStoreListener.OnInitialized(IStoreController controller, IExtensionProvider extensions)
    {
        Debug.Log("IAPManager: OnInitialized");
        this.controller = controller;
        this.extensions = extensions;

        try
        {
            int total = controller.products.all.Length;
            Debug.Log($"IAPManager: products registered = {total}");
        }
        catch (Exception ex)
        {
            Debug.LogWarning("IAPManager: exception reading products after init: " + ex);
        }
    }

    void IStoreListener.OnInitializeFailed(InitializationFailureReason error)
    {
        Debug.LogError($"IAPManager: OnInitializeFailed - {error}");
        controller = null;
        extensions = null;
    }

    void IStoreListener.OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
    {
        Debug.LogWarning($"IAPManager: OnPurchaseFailed - {product?.definition?.id} reason: {failureReason}");
    }

    public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
    {
        Debug.Log("receipt:  " + args.purchasedProduct.receipt);
        Debug.Log("transactionID:  " + args.purchasedProduct.transactionID);
        JObject receiptObj = JObject.Parse(args.purchasedProduct.receipt);
        if (((string)receiptObj["Store"]).Equals("fake")) return PurchaseProcessingResult.Complete;

#if UNITY_ANDROID
        SocketSend.sendIAPResult(args.purchasedProduct.receipt);
#else
        SocketSend.validateIAPReceipt(args.purchasedProduct.receipt);
#endif
        Debug.Log("xem là mua thành công hay ko" + args.purchasedProduct.definition.id + " " + PurchaseProcessingResult.Complete);
        return PurchaseProcessingResult.Complete;
    }

    public void OnInitializeFailed(InitializationFailureReason error, string message)
    {
        Debug.LogError($"IAP OnInitializeFailed: {error} - {message}");
        controller = null;
        extensions = null;
    }
}