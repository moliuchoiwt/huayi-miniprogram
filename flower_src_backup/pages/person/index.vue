<template>
	<view class="page" v-if="bannerObj['10002']">
		<image class="bg" :src="bannerObj['10002'].imgUrl" mode="widthFix"></image>
		<view class="box">
			<up-navbar title="我的" bgColor='#ffffff00' title-color="#000" leftIcon='' :fixed="false" placeholder />
			<view class="userInfo" @click="href('/pagesPerson/info', true)">
				<image class="avatar" :src="storeUser.user.avatar || '/static/logo.png'" mode="aspectFill"></image>
				<view class="right" v-if="storeUser.user.userId">
					<view class="name">
						<view class="text">{{ storeUser.user.nickName }}</view>
					</view>
					<view class="info">
						<view class="phone">
							{{ storeUser.user.mobile }}
						</view>
					</view>
				</view>
				<view class="right" v-else>
					<view class="LoginTitle">快速登录</view>
				</view>
			</view>
			<!-- 我的收益 -->
			<view class="profitBox" v-if="storeUser.user.userId">
				<image :src="bannerObj['10003'].imgUrl" mode="widthFix" style="width: 100%;"></image>
				<view class="box">
					<view class="left">
						<view class="num">我的收益</view>
						<view class="price">{{ wallet.amount.toFixed(2) }}</view>
					</view>
					<view class="btn" @click="href('/pagesPerson/withdraw')">提现</view>
				</view>
			</view>
			<view class="square" style="margin: 24rpx 0;" v-if="storeUser.user.shopAuditStatus == 1">
				<view class="servceList2">
					<view class="item" @click="href('/pages2ShopPort/person/index')" style="padding: 0;">
						<view class="left">
							<image src="/static/icon05.png" mode=""></image>
							<view class="text" style="font-weight: 600;font-size: 32rpx;">我的花店</view>
						</view>
						<up-icon name="arrow-right"></up-icon>
					</view>
				</view>
			</view>
			<view class="square">
				<view class="title">
					<view class="name">服务中心</view>
				</view>
				<!-- <view class="servceList">
					<view v-for="item, index in servceList" :key="index" @click="to(item, index)">
						<view class="item" v-if="item[1] != '客服中心'">
							<image :src="item[0]" mode=""></image>
							<view class="text">{{ item[1] }}</view>
						</view>
						<button open-type="contact" v-else>
							<view class="item">
								<image :src="item[0]" mode=""></image>
								<view class="text">{{ item[1] }}</view>
							</view>
						</button>
					</view>
				</view> -->
				<view class="servceList2">
					<view v-for="item, index in servceList" :key="index" @click="to(item, index)">
						<view class="item" v-if="item[1] != '客服中心'">
							<view class="left">
								<image :src="item[0]" mode=""></image>
								<view class="text">{{ item[1] }}</view>
							</view>
							<up-icon name="arrow-right"></up-icon>
						</view>
						<button open-type="contact" v-else>
							<view class="item">
								<view class="left">
									<image :src="item[0]" mode=""></image>
									<view class="text">{{ item[1] }}</view>
								</view>
								<up-icon name="arrow-right"></up-icon>
							</view>
						</button>
					</view>
				</view>
			</view>
			<view class="divider">
				<up-divider text="花艺" lineColor="#999999"></up-divider>
			</view>
			<view style="height: 32rpx;"></view>
		</view>
	</view>
</template>

<script>
export default {
	data() {
		return {
			bannerObj: {},
			// 服务中心
			servceList: [
				['/static/icon17.png', "收益明细", "/pagesPerson/profit"],
				// ['/static/icon18.png', "地址管理", "/pagesPerson/address/index"],
				['/static/icon19.png', "客服中心", ""],
				['/static/icon20.png', "隐私条例", ""],
				['/static/icon21.png', "意见反馈", "/pagesPerson/feedback"],
			],
			wallet: {
				amount: 0
			},
		};
	},
	onLoad() {
		// 背景图  
		this.request(`news/BannerList`, {
			Ids: [10002, 10003]
		}).then(res => {
			this.bannerObj = res.data.items
		})
	},
	onShow() {
		this.storeUser.updateUser()
		if (this.getToken()) {
			this.getMyWallet()
		}
	},
	methods: {
		async getMyWallet() {
			let res = await this.request('Money/MyWallet', {
				queryType: 0,
			})
			this.wallet = res.data
		},
		to(item, index) {
			if (item[1] == '客服中心') {

			} else if (item[1] == '隐私条例') {
				uni.openPrivacyContract()
			} else {
				this.href(item[2], true)
			}
		},
	}
}
</script>

<style lang="scss" scoped>
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

.page {
	min-height: 100vh;
	width: 750rpx;
}

.bg {
	width: 750rpx;
}

.box {
	position: absolute;
	width: 100%;
	left: 0;
	top: 0;
	padding: 0 40rpx;
}

.userInfo {
	margin: 32rpx 0;
	display: flex;
	align-items: center;

	.avatar {
		width: 112rpx;
		height: 112rpx;
		border-radius: 50%;
	}

	.right {
		margin-left: 24rpx;
		flex: 1;

		.name {
			font-weight: 500;
			font-size: 36rpx;
			color: #333333;
			display: flex;
			align-items: center;

			image {
				margin-left: 1rpx;
				width: 40rpx;
				height: 40rpx;
			}
		}

		.info {
			margin-top: 12rpx;
			display: flex;
			align-items: center;

			.star {
				display: flex;
				align-items: center;
				background: linear-gradient(270deg, #E5BE7D 0%, #E1A770 100%);
				border-radius: 28rpx;
				padding: 6rpx 16rpx;

				.text {
					margin-left: 2rpx;
					font-size: 20rpx;
					color: #FFFFFF;
				}
			}

			.phone {
				// margin-left: 16rpx;
				font-size: 24rpx;
				color: #999999;
			}
		}

		.LoginTitle {
			display: flex;
			height: 100%;
			align-items: center;
			color: #8C4FFF;
			font-size: 34rpx;
		}

	}
}

.profitBox {
	position: relative;

	.box {
		display: flex;
		justify-content: space-between;
		align-items: center;
		height: 100%;

		.left {
			.num {
				font-size: 24rpx;
				color: #FFFFFF;
			}

			.price {
				margin-top: 14rpx;
				font-weight: 600;
				font-size: 48rpx;
				color: #FFFFFF;
			}
		}

		.btn {
			width: 152rpx;
			height: 64rpx;
			background: #FFFFFF;
			border-radius: 16rpx;
			display: flex;
			justify-content: center;
			align-items: center;
			font-size: 28rpx;
			color: #8C4FFF;
		}
	}
}

.square {
	margin-top: 24rpx;
	padding: 24rpx;
	border-radius: 16rpx;
	background: white;

	.title {
		display: flex;
		justify-content: space-between;
		background: white;

		.name {
			font-weight: 600;
			font-size: 28rpx;
			color: #333333;
		}
	}

	.orderList {
		padding-top: 28rpx;
		display: flex;

		.item {
			width: 25%;
			display: flex;
			flex-direction: column;
			align-items: center;
			justify-content: center;
			position: relative;

			image {
				width: 56rpx;
				height: 56rpx;
			}

			.name {
				font-size: 28rpx;
				color: #000000;
				margin-top: 4rpx;
			}
		}
	}

	.servceList {
		display: flex;
		flex-wrap: wrap;

		>view {
			width: 25%;
			margin-top: 32rpx;

			.item {
				width: 100%;
				display: flex;
				flex-direction: column;
				justify-content: center;
				align-items: center;

				image {
					width: 48rpx;
					height: 48rpx;
				}

				.text {
					margin-top: 16rpx;
					font-size: 30rpx;
					color: #333333;
				}
			}
		}
	}

	.servceList2 {
		.item {
			padding: 30rpx 0;
			display: flex;
			align-items: center;
			justify-content: space-between;

			.left {
				display: flex;
				align-items: center;

				image {
					width: 56rpx;
					height: 56rpx;
				}

				.text {
					font-size: 28rpx;
					color: #333333;
					margin-left: 16rpx;
				}
			}
		}
	}
}

.divider {
	padding: 24rpx 200rpx;
}
</style>