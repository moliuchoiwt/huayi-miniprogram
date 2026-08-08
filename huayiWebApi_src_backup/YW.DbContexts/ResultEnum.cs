namespace YW.DbContexts
{
    /// <summary>
    /// 返回结果Code
    /// </summary>
    public enum ResultEnum
    {
        success = 200,
        fail = 400,
        notAddress = 210,
        notLogin = 501,
    }

    /// <summary>
    /// 账户类型
    /// </summary>
    public enum walletTypeEnum
    {
        余额 = 0,
        积分 = 1,
        佣金 = 2
    }
    /// <summary>
    /// 资金用户类型
    /// </summary>
    public enum walletUserTypeEnum
    {
        用户 = 0
    }

    /// <summary>
    /// 来源类型
    /// </summary>
    public enum sourceTypeEnum
    {
        平台变更 = 0,
        消费抵扣 = 1,
        任务奖励 = 2,
        订单退款 = 3,
        邀请奖励 = 4,
        订单购买 = 5,
        礼包奖励 = 6,
        商品直推 = 7,
        商品间推 = 8,

        充值 = 11,
        提现 = 12
    }



    /// <summary>
    /// 验证码类型
    /// </summary>
    public enum SmsEnum
    {

        注册 = 0,
        登录 = 1,
    }

    /// <summary>
    /// 订单类型  1-购物订单 
    /// </summary>
    public enum OrderEnum
    {

        购物订单 = 1
    }

    /// <summary>
    /// 订单状态 0-未支付 9-已完成 
    /// </summary>
    public enum OrderStateEnum
    {
        已完成 = 9,
        待支付 = 0,
        待发货 = 1,
        待收货 = 2,
        待评论 = 3,
        待售后 = 4,
        已售后 = 5,
        待核销 = 6,
    }


    /// <summary>
    /// 支付方式 微信JSAPI = 0,微信NATIVE = 1, 微信APP = 2,微信MWEB = 3 ,支付宝APP=4,余额支付=5
    /// </summary>
    public enum PayEnum
    {
        微信JSAPI = 0,
        微信NATIVE = 1,
        微信APP = 2,
        微信MWEB = 3,
        支付宝APP = 4,
        余额支付 = 5,
        线下支付 = 9,
        无需支付 = 10,
        积分支付 = 10
    }



    /// <summary>
    /// 商品类型 0-普通商品 
    /// </summary>
    public enum GoodsTypeEnum
    {
        普通商品 = 0
    }



}
