<template>
	<view>
		<!-- 登录 -->
		<button class="btn" @getphonenumber="iphoneLogin" open-type="getPhoneNumber">手机号快捷注册登录</button>
		<view class="clause">
			<up-checkbox v-model:checked="checked" active-color="#db3327" usedAlone></up-checkbox>
			<view>
				我已阅读并同意<text @click="href(`/pages/common/richText?type=1`)">《隐私条款》</text>协议并注册
			</view>
		</view>
		<!-- 
		<view class="empty" v-if="list.length<=0">
					<image src="/static/img32.png" mode="widthFix"></image>
				</view>
		-->
		<!-- 头像 -->
		<button open-type="chooseAvatar" @chooseavatar="chooseAvatar">

		</button>
		<!-- 客服 -->
		<button open-type="contact"></button>
		<!-- 分享 -->
		<button open-type="share"></button>
		<!-- 选择时间段 19:00 - 21:22 this.$refs.timeslot.open()-->
		<timeSlot ref="timeslot" :title="'选择时间段'" @confirm="confirmTime">
		</timeSlot>
	</view>
</template>

<script>
	import {
		useUserStore
	} from '/store/user'
	import timeSlot from "@/components/wanghexu-timeslot/wanghexu-timeslot.vue"
	export default {
		data() {
			return {
				storeUser:useUserStore(),
				checked: false,
				model: {
					encryptedData: '',
					iv: '',
					code: '',
					parentId: 0,
					getWxPhoneCode:""
				},
				query: {
					pageNum: 1,
					pageSize: 10,
					queryName: ""
				},
				total: 0,
				list: [],
			}
		},
		components: {
			timeSlot
		},
		async onReachBottom() {
			if (this.total > this.query.pageNum * this.query.pageSize) {
				this.query.pageNum += 1
				await this.getList()
			}
		},
		methods: {
			async getList(reset) {
				if (reset) this.query.pageNum = 1
				let res = await this.request('Goods/GoodsList', this.query)
				if (this.query.pageNum == 1) {
					this.list = res.data.items
					this.total = res.data.total
				} else {
					this.list.push(...res.data.items)
				}
			},
			async iphoneLogin(e) {
				console.log('输出', e)
				if (!this.checked) {
					this.toast('请勾选隐私条款协议')
					return
				}
				this.model.encryptedData = e.target.encryptedData
				this.model.iv = e.target.iv
				this.model.getWxPhoneCode = e.target.code
				uni.login({
					provider: 'weixin',
					success: async (loginRes) => {
						if (loginRes && loginRes.code) {
							this.model.code = loginRes.code
							const res = await this.request("Login/WxOpenMobileLogin", this.model)
							if (res.code == 200) {
								this.setToken(res.data)
								await storeUser.updateUser()
								//开启websocket
								// storeUser.connectSocketInit()
							} else {
								this.toast(res.msg)
							}
						}
					}
				})
			},
			// 选择头像
			async chooseAvatar(e) {
				const {
					avatarUrl
				} = e.detail
				console.log(avatarUrl);
			},
			confirmTime(e) {
				console.log(e);
				// this.query.toDoorTimeStart = e.start.hour + ':' + e.start.min
				// this.query.toDoorTimeEnd = e.end.hour + ':' + e.end.min
				// {start:{hour:'00',min:'00'},end:{hour:'00',min:'00'}}
			},
			// 转账确认
			immediately(item) {
				console.log(item);
				if (wx.canIUse('requestMerchantTransfer')) {
					wx.requestMerchantTransfer({
						mchId: this.mch_id,
						appId: this.WxOpenAppId,
						package: item.package,
						success: (res) => {
							// res.err_msg将在页面展示成功后返回应用时返回ok，并不代表付款成功
							console.log('success:', res);
							this.getData()
							this.getData2()
						},
						fail: (res) => {
							console.log('fail:', res);
						},
					});
				} else {
					wx.showModal({
						content: '你的微信版本过低，请更新至最新版本。',
						showCancel: false,
					});
				}
			},
		},
		onShareAppMessage() {
			return {
				title: '名称',
				path: '/pages/index/index'
			}
		}
	}
</script>

<style>
	button {
		padding: 0;
		background-color: initial;
		margin: 0;
		line-height: initial;
		overflow: initial;
		box-sizing: initial;
		font-size: initial;
		border: 0;

	}

	button::after {
		content: none;
	}
	
	/* 瀑布流布局 
		column-count: 2;
		column-gap: 32px;
		column-fill: auto; 
		子元素
		break-inside: avoid;
	*/
	/* 不改变盒子大小
	   flex-shrink: 0;
	 */
</style>