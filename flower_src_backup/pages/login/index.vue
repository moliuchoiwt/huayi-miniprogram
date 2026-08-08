<template>
	<view class="page">
		<view class="imgBg">
			<!-- <image src="/static/image01.png"></image> -->
		</view>
		<view class="box">
			<view class="arrowLeft" @click="back()">
				<up-icon name="arrow-left" size="46rpx"></up-icon>
			</view>
			<image src="/static/logo.png"></image>
			<button class="btn" @getphonenumber="iphoneLogin" open-type="getPhoneNumber">手机号快捷注册登录</button>
			<view class="clause">
				<up-checkbox v-model:checked="checked" active-color="#8C4FFF " usedAlone></up-checkbox>
				<view>
					我已阅读并同意<text @click="toRichText">《隐私条款》</text>协议并注册
				</view>
			</view>
		</view> 
	</view>
</template>

<script>
export default {
	data() {
		return {
			checked: false,
			model: {
				encryptedData: '',
				iv: '',
				code: '',
				getWxPhoneCode: "",
				parentId: 0
			},
		}
	},
	onLoad(options) {
		if (options.userId) {
			this.model.parentId = options.userId
			uni.setStorageSync('pId', options.userId)
		}
		if (this.getToken()) {
			uni.reLaunch({
				url: '/pages/index/index'
			})
		} 
	},
	methods: {
		async iphoneLogin(e) {
			console.log('输出', e)
			if (!this.checked) {
				this.toast('请勾选隐私条款协议')
				return
			}
			// 	// return
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
							await this.storeUser.updateUser()
							this.back()
						} else {
							this.toast(res.msg)
						}
					}
				}
			})
		},
		toRichText() {
			uni.openPrivacyContract()
		}
	}
}
</script>

<style lang="scss" scoped>
.page {
	position: relative;
	max-height: 100vh;
}

.imgBg {
	width: 100%;
	height: 100vh;

	image {
		width: 100%;
		height: 100%;
	}
}

.box {
	position: absolute;
	left: 0;
	top: 0;
	width: 100%;
	margin-top: 340rpx;
	display: flex;
	flex-direction: column;
	align-items: center;

	.arrowLeft {
		position: fixed;
		left: 32rpx;
		top: 100rpx;
	}

	image {
		width: 256rpx;
		height: 256rpx;
		border-radius: 40rpx;
	}

	.title {
		margin-top: 30rpx;
		font-size: 40rpx;
		color: #111111;
	}

	.btn {
		margin-top: 154rpx;
		width: 622rpx;
		height: 88rpx;
		background: #8C4FFF;
		border-radius: 16rpx;
		font-size: 36rpx;
		color: #fff;
		display: flex;
		justify-content: center;
		align-items: center;
	}

	.clause {
		margin-top: 47rpx;
		display: flex;
		align-items: center;
		font-size: 30rpx;
		color: #222900;

		text {
			color: #EB3B3A;
		}
	}
}
</style>